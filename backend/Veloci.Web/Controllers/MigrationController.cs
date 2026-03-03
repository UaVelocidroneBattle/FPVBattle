using Microsoft.AspNetCore.Mvc;

namespace Veloci.Web.Controllers;

[ApiController]
[Route("/api/migration/[action]")]
public class MigrationController
{
    [HttpGet("/api/migration/something")]
    public async Task MigrateSomething()
    {
        // Here migrate something
    }
}
