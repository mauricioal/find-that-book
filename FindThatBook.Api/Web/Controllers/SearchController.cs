using FindThatBook.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    /// Searches for books based on a fuzzy user query.
    /// </summary>
    /// <remarks>
    /// This endpoint uses Gemini AI to extract intent from the query, 
    /// then searches OpenLibrary and ranks the results using a deterministic matcher.
    /// </remarks>
    /// <param name="query">The messy search query (e.g., "blue book about magic by sanderson"). Max 250 characters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of up to 5 ranked book candidates with AI-generated explanations.</returns>
    /// <response code="200">Returns the list of matching books.</response>
    /// <response code="400">If the query is empty or exceeds the length limit.</response>
    /// <response code="429">If the rate limit is exceeded (10 requests per minute).</response>
    [HttpGet]
    [EnableRateLimiting("SearchPolicy")]
    [ProducesResponseType(typeof(IEnumerable<FindThatBook.Api.Domain.Entities.BookCandidate>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Search([FromQuery] string query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query cannot be empty.");
        if (query.Length > 250) return BadRequest("Query is too long. Maximum length is 250 characters.");

        var results = await _searchService.SearchAsync(query, ct);
        return Ok(results);
    }
}
