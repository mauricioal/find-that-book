using FindThatBook.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindThatBook.Api.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IBookSearchService _searchService;

    public SearchController(IBookSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query cannot be empty.");

        var results = await _searchService.SearchAsync(query, ct);
        return Ok(results);
    }
}
