using FluentAssertions;
using Veloci.Data.Domain;

namespace Veloci.Tests;

public class PilotLeagueTests
{
    private const string CupId = "open-class";

    [Fact]
    public void get_current_league_without_date_should_ignore_historical_records()
    {
        var pilot = new Pilot("TestPilot")
        {
            Leagues =
            [
                new PilotLeague { CupId = CupId, Date = new DateTime(2026, 6, 1), League = "Silver", Status = LeagueRecordStatus.Historical },
                new PilotLeague { CupId = CupId, Date = new DateTime(2026, 7, 1), League = null, Status = LeagueRecordStatus.Current }
            ]
        };

        pilot.GetCurrentLeague(CupId).Should().BeNull();
    }

    [Fact]
    public void get_current_league_for_date_after_retirement_should_return_null()
    {
        // Pilot was promoted to Silver on June 1st, then retired from all leagues on July 1st.
        var pilot = new Pilot("TestPilot")
        {
            Leagues =
            [
                new PilotLeague { CupId = CupId, Date = new DateTime(2026, 6, 1), League = "Silver", Status = LeagueRecordStatus.Historical },
                new PilotLeague { CupId = CupId, Date = new DateTime(2026, 7, 1), League = null, Status = LeagueRecordStatus.Current }
            ]
        };

        pilot.GetCurrentLeague(CupId, new DateTime(2026, 7, 2)).Should().BeNull();
    }

    [Fact]
    public void get_current_league_for_date_before_retirement_should_return_last_assigned_league()
    {
        var pilot = new Pilot("TestPilot")
        {
            Leagues =
            [
                new PilotLeague { CupId = CupId, Date = new DateTime(2026, 6, 1), League = "Silver", Status = LeagueRecordStatus.Historical },
                new PilotLeague { CupId = CupId, Date = new DateTime(2026, 7, 1), League = null, Status = LeagueRecordStatus.Current }
            ]
        };

        pilot.GetCurrentLeague(CupId, new DateTime(2026, 6, 15)).Should().Be("Silver");
    }
}