using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Application.Services;
using FindThatBook.Api.Infrastructure.AI;
using FindThatBook.Api.Infrastructure.OpenLibrary;
using Microsoft.Extensions.AI;
using GeminiDotnet.Extensions.AI;
using GeminiDotnet;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// AI Configuration
var geminiApiKey = builder.Configuration["Gemini:ApiKey"] ?? "YOUR_API_KEY_HERE";
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
