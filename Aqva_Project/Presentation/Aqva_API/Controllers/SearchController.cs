using Application.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Aqva_API.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class SearchController(IColumnistRepository columnistRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var search = await columnistRepository.SearchAsync(query);
        return Ok(search);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var search = await columnistRepository.GetAllColumnistsAsync();
        return Ok(search);
    }
}