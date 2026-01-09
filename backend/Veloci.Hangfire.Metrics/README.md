# Hangfire Metrics Instrumentation for OpenTelemetry .NET

## Overview

This is a **Metrics Instrumentation Library** for [Hangfire](https://www.hangfire.io/) that collects performance and operational telemetry about background job execution. It provides comprehensive visibility into job lifecycle metrics including creation, queuing, execution, and completion.

This instrumentation complements the official [OpenTelemetry.Instrumentation.Hangfire](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.Hangfire) package, which provides **tracing** support. While the official package tracks individual job execution traces, this library focuses on **metrics** for monitoring job performance, throughput, and operational health.

## Status

- **Maturity**: Experimental
- **Target Framework**: .NET Standard 2.1

## Features

This instrumentation automatically collects the following metrics:

| Metric Name | Type | Unit | Description |
|-------------|------|------|-------------|
| `hangfire.job.created` | Counter | `{job}` | Total number of jobs created/enqueued |
| `hangfire.job.executions` | Counter | `{job}` | Total number of job executions (success + failed) |
| `hangfire.job.active` | UpDownCounter | `{job}` | Number of currently executing jobs |
| `hangfire.job.duration` | Histogram | `s` | Duration of job execution in seconds |
| `hangfire.job.queue.latency` | Histogram | `s` | Time jobs spend waiting in queue before execution starts |

### Metric Attributes

All metrics include the following attributes for detailed filtering and aggregation:

| Attribute | Description | Example |
|-----------|-------------|---------|
| `job.type` | Fully qualified job class name | `Veloci.Logic.Jobs.CompetitionCheckJob` |
| `job.method` | Job method name | `Execute` |
| `job.name` | Human-readable job signature | `CompetitionCheckJob.Execute` |
| `job.status` | Execution status (execution metrics only) | `success`, `failed` |
| `exception.type` | Exception type when failed (execution metrics only) | `System.InvalidOperationException` |

## Installation

### Prerequisites

1. **Hangfire** - Install and configure Hangfire in your application
2. **OpenTelemetry SDK** - Required for metrics collection

### Step 1: Install Package Dependencies

Since this is an internal library, add a project reference:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\Veloci.Hangfire.Metrics\Veloci.Hangfire.Metrics.csproj" />
</ItemGroup>
```

Or copy the library files directly into your project.

### Step 2: Install OpenTelemetry Packages

```bash
dotnet add package OpenTelemetry
```

## Getting Started

### Basic Configuration

Add the Hangfire metrics instrumentation to your OpenTelemetry configuration:

```csharp
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddHangfireInstrumentation()  // Enable Hangfire metrics
        .AddPrometheusExporter());      // Export to Prometheus

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.Run();
```

### ASP.NET Core Integration

For ASP.NET Core applications, configure in `Startup.cs` or `Program.cs`:

```csharp
public void ConfigureBuilder(WebApplicationBuilder builder)
{
    var otel = builder.Services.AddOpenTelemetry();

    otel.ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName));

    otel.WithMetrics(metrics => metrics
        // OpenTelemetry built-in instrumentations
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()

        // Hangfire metrics instrumentation
        .AddHangfireInstrumentation()

        // Export to Prometheus
        .AddPrometheusExporter());
}
```

## Usage Examples

### Monitor Job Creation Rate

Track how many jobs are being enqueued:

```promql
# Jobs created per second
rate(hangfire_job_created_total[5m])

# Jobs created by job type
sum by (job_type) (rate(hangfire_job_created_total[5m]))
```

### Detect Job Backlog

Identify when jobs are being created faster than they're executed:

```promql
# Backlog: jobs created but not yet executed
hangfire_job_created_total - hangfire_job_executions_total

# Alert when backlog exceeds threshold
(hangfire_job_created_total - hangfire_job_executions_total) > 100
```

### Monitor Job Success Rate

Track the percentage of successful job executions:

```promql
# Success rate (0-1)
rate(hangfire_job_executions_total{job_status="success"}[5m])
  / rate(hangfire_job_executions_total[5m])

# Failure rate by job type
sum by (job_type) (rate(hangfire_job_executions_total{job_status="failed"}[5m]))
```

### Track Job Performance

Monitor job execution duration using percentiles:

```promql
# P95 execution time across all jobs
histogram_quantile(0.95, rate(hangfire_job_duration_bucket[5m]))

# P95 execution time by job type
histogram_quantile(0.95,
  sum by (job_type, le) (rate(hangfire_job_duration_bucket[5m])))

# Average execution time
rate(hangfire_job_duration_sum[5m]) / rate(hangfire_job_duration_count[5m])
```

### Monitor Queue Latency

Identify when jobs are waiting too long before execution:

```promql
# P99 queue wait time
histogram_quantile(0.99, rate(hangfire_job_queue_latency_bucket[5m]))

# Alert on high queue latency (> 30 seconds)
histogram_quantile(0.95, rate(hangfire_job_queue_latency_bucket[5m])) > 30
```

### Track Active Jobs

Monitor worker saturation:

```promql
# Currently executing jobs
hangfire_job_active

# Active jobs by type
sum by (job_type) (hangfire_job_active)

# Alert when all workers are busy (adjust based on worker count)
hangfire_job_active >= 20
```

## Architecture

### How It Works

The instrumentation uses Hangfire's filter mechanism to hook into the job lifecycle:

1. **Job Creation** - `IClientFilter` tracks when jobs are enqueued via `BackgroundJob.Enqueue()`
2. **Job Execution** - `IServerFilter` tracks when workers pick up and execute jobs
3. **Metrics Collection** - Uses `System.Diagnostics.Metrics` API for efficient metric recording
4. **OpenTelemetry Integration** - Metrics are automatically exported to configured exporters

### Filter Pipeline

```
Job Creation Flow:
  BackgroundJob.Enqueue()
    → IClientFilter.OnCreating()
    → IClientFilter.OnCreated() [hangfire.job.created++]
    → Job stored in queue

Job Execution Flow:
  Worker picks job
    → IServerFilter.OnPerforming() [hangfire.job.active++]
    → Job executes
    → IServerFilter.OnPerformed() [hangfire.job.executions++,
                                     hangfire.job.duration recorded,
                                     hangfire.job.active--,
                                     hangfire.job.queue.latency recorded]
```

## Use Cases

### 1. SLA Monitoring

Alert when job execution time exceeds SLA thresholds:

```yaml
# Prometheus alert rule
- alert: HangfireJobSLAViolation
  expr: histogram_quantile(0.95, rate(hangfire_job_duration_bucket[5m])) > 5
  annotations:
    summary: "95th percentile job execution time exceeds 5 seconds"
```

### 2. Capacity Planning

Determine if you need more workers:

```promql
# Worker utilization (active jobs / total workers)
hangfire_job_active / 20  # Assuming 20 workers

# Job throughput
rate(hangfire_job_executions_total[1h])
```

### 3. Performance Regression Detection

Track job duration trends over deployments:

```promql
# Compare current vs previous week
(rate(hangfire_job_duration_sum[1h]) / rate(hangfire_job_duration_count[1h]))
  /
(rate(hangfire_job_duration_sum[1h] offset 1w) / rate(hangfire_job_duration_count[1h] offset 1w))
```

### 4. Bottleneck Identification

Find jobs that spend too much time in queue vs execution:

```promql
# Queue time vs execution time ratio
(rate(hangfire_job_queue_latency_sum[5m]) / rate(hangfire_job_queue_latency_count[5m]))
  /
(rate(hangfire_job_duration_sum[5m]) / rate(hangfire_job_duration_count[5m]))
```

## Advanced Topics

### Custom Histogram Buckets

Customize histogram buckets for your job duration patterns:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddHangfireInstrumentation()
        .AddView(
            instrumentName: "hangfire.job.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0.1, 0.5, 1, 2, 5, 10, 30, 60 }
            }));
```

### Filtering Metrics by Job Type

Reduce cardinality by filtering specific job types:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddHangfireInstrumentation()
        .AddView(
            instrumentName: "hangfire.job.duration",
            new MetricStreamConfiguration
            {
                TagKeys = new[] { "job.type", "job.status" }
            }));
```

### Disable Specific Metrics

If you only need certain metrics, filter at the exporter level:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddHangfireInstrumentation()
        .AddView("hangfire.job.created", MetricStreamConfiguration.Drop)
        .AddPrometheusExporter());
```

## Grafana Dashboard

### Sample Dashboard Panels

#### Job Throughput
```promql
# Panel: Job Creation Rate
rate(hangfire_job_created_total[5m])

# Panel: Job Execution Rate
rate(hangfire_job_executions_total[5m])
```

#### Job Success Rate
```promql
# Panel: Success Rate (%)
100 * (
  rate(hangfire_job_executions_total{job_status="success"}[5m])
  / rate(hangfire_job_executions_total[5m])
)
```

#### Active Jobs
```promql
# Panel: Currently Executing Jobs
hangfire_job_active
```

#### Job Duration Heatmap
```promql
# Panel: Job Duration Distribution
sum by (le) (rate(hangfire_job_duration_bucket[5m]))
```

## Troubleshooting

### Metrics Not Appearing

1. **Verify meter is added**: Ensure `AddHangfireInstrumentation()` is called
2. **Check meter name**: The meter name is `Veloci.Hangfire` (version `1.0.0`)
3. **Verify filters are registered**: Check that `GlobalJobFilters.Filters` contains both filters
4. **Test with console exporter**: Use `AddConsoleExporter()` to verify metrics are being collected

### High Cardinality

If you have too many unique job types causing cardinality issues:

1. **Use views to drop dimensions**: Remove `job.method` or `job.name` attributes
2. **Filter specific job types**: Only track critical jobs
3. **Aggregate at query time**: Use Prometheus recording rules

## Comparison with Official Package

| Feature | OpenTelemetry.Instrumentation.Hangfire | Veloci.Hangfire.Metrics |
|---------|----------------------------------------|-------------------------|
| **Tracing** | ✅ Complete traces with spans | ❌ Not supported |
| **Metrics** | ❌ Not supported | ✅ 5 comprehensive metrics |
| **Job Creation Tracking** | ❌ No | ✅ `hangfire.job.created` |
| **Queue Latency** | ❌ No | ✅ `hangfire.job.queue.latency` |
| **Active Jobs Count** | ❌ No | ✅ `hangfire.job.active` |
| **Maturity** | Beta | Experimental |
| **NuGet Package** | ✅ Official | ❌ Internal library |

## References

- [Hangfire Official Documentation](https://docs.hangfire.io/)
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
- [OpenTelemetry .NET Contrib](https://github.com/open-telemetry/opentelemetry-dotnet-contrib)
- [OpenTelemetry Metrics Specification](https://opentelemetry.io/docs/specs/otel/metrics/)
- [Prometheus Query Language](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [System.Diagnostics.Metrics API](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics)
