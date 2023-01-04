using HenrymedsReservation.Models;
using Microsoft.AspNetCore.Mvc;

namespace HenrymedsReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProviderController : ControllerBase
{
    [HttpPost("PostAvailablePeriods", Name = "PostAvailablePeriods")]
    public IActionResult PostAvailablePeriods(
        [FromQuery] int pid,
        [FromBody] IEnumerable<Calendar.TimeRange> availablePeriods)
    {
        try
        {
            var providerId = new ProviderId(pid);
            foreach (var timeRange in availablePeriods)
            {
                Calendar.ValidateTimeRange(timeRange);
            }
            Calendar.AddTimeRanges(providerId, availablePeriods);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}