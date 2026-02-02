using FindThatBook.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindThatBook.Api.Web.Controllers;

/// <summary>
/// Handles HTTP requests for book search operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IBookSearchService _searchService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchController"/> class.
    /// </summary>
    /// <param name="searchService">The service used to perform book searches.</param>
    public SearchController(IBookSearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Searches for books based on a user query string.
    /// </summary>
    /// <param name="query">The search query (e.g., "fantasy books by tolkien").</param>
    /// <param name="ct">A cancellation token to cancel the request.</param>
    /// <returns>An HTTP response containing the search results or an error message.</returns>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query cannot be empty.");

        var results = await _searchService.SearchAsync(query, ct);
        return Ok(results);
    }
}
