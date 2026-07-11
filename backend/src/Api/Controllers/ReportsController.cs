using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("totals")]
    [ProducesResponseType(typeof(TotalsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotals()
    {
        var totals = await _reportService.GetTotalsAsync();
        return Ok(totals);
    }
}
