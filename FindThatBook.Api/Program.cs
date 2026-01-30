using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Application.Services;
using FindThatBook.Api.Infrastructure.AI;
using FindThatBook.Api.Infrastructure.OpenLibrary;
using Microsoft.Extensions.AI;
using GeminiDotnet.Extensions.AI;
using GeminiDotnet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<IBookSearchService, BookSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
