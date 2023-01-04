using HenrymedsReservation.Models;
using Microsoft.AspNetCore.Mvc;

namespace HenrymedsReservation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    [HttpGet("ListAvailableSlots")]
    public IActionResult ListAvailableSlots(int pid)
    {
        try
        {
            var provider = new ProviderId(pid);
            return Ok(Calendar.GetAvailableSlots(provider));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Reserve")]
    public IActionResult Reserve(
        [FromQuery] int cid,
        [FromQuery] int pid,
        [FromBody] DateTimeOffset slot)
    {
        try
        {
            var client = new ClientId(cid);
            var provider = new ProviderId(pid);
            Calendar.ReserveSlot(client, provider, Calendar.ValidateDTO(slot));
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Confirm")]
    public IActionResult Confirm(
        [FromQuery] int cid,
        [FromQuery] int pid,
        [FromBody] DateTimeOffset slot)
    {
        try
        {
            var provider = new ProviderId(pid);
            var client = new ClientId(cid);
            Calendar.ConfirmSlot(client, provider, Calendar.ValidateDTO(slot));
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
