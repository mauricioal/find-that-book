using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Application.Services;
using FindThatBook.Api.Infrastructure.AI;
using FindThatBook.Api.Infrastructure.OpenLibrary;
using Microsoft.Extensions.AI;
using GeminiDotnet.Extensions.AI;
using GeminiDotnet;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FindThatBook.Api.Application.DTOs;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuration
builder.Services.Configure<ExternalServicesConfig>(
    builder.Configuration.GetSection("ExternalServices"));
builder.Services.Configure<GeminiConfig>(
    builder.Configuration.GetSection("Gemini"));

builder.Services.AddSwaggerGen(options =>
{
    var config = builder.Configuration.GetSection("ExternalServices").Get<ExternalServicesConfig>();
    
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Find That Book API",
        Version = "v1",
        Description = "A smart library discovery service that uses Google Gemini AI to interpret messy user queries and find matches via OpenLibrary.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Find That Book Support",
            Url = new Uri(config?.Project?.RepoUrl ?? "https://github.com/mauricioal/find-that-book")
        }
    });

    // Include XML comments in Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "SearchPolicy", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

builder.Services.AddCors(options =>
{
    var config = builder.Configuration.GetSection("ExternalServices").Get<ExternalServicesConfig>();
    
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(config?.Frontend?.BaseUrl ?? "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// AI Configuration
var geminiConfig = builder.Configuration.GetSection("Gemini").Get<GeminiConfig>();
var geminiApiKey = geminiConfig?.ApiKey ?? builder.Configuration["Gemini:ApiKey"] ?? "YOUR_API_KEY_HERE";

builder.Services.AddSingleton<IChatClient>(new GeminiChatClient(new GeminiClientOptions 
{ 
    ApiKey = geminiApiKey,
    ModelId = "gemini-2.5-flash"
}));

// Infrastructure & Application
builder.Services.AddHttpClient<IOpenLibraryClient, OpenLibraryClient>();
builder.Services.AddScoped<IAiService, GeminiAiService>();
builder.Services.AddScoped<IBookMatcher, BookMatcher>();
builder.Services.AddScoped<IBookSearchService, BookSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
