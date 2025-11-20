using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLWeb.API;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// 🧠 Explicitly reload config from appsettings.json (and environment-specific file)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// ✅ CORS policy for React/Swagger/local calls
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OpenAiSettings>(
    builder.Configuration.GetSection("OpenAI"));


var app = builder.Build();

Console.WriteLine("📁 WORKING DIR = " + Directory.GetCurrentDirectory());
Console.WriteLine("📄 appsettings.json exists? " + File.Exists("appsettings.json"));
Console.WriteLine("📄 FULL PATH = " + Path.GetFullPath("appsettings.json"));


// ✅ Enable CORS first
app.UseCors("AllowAll");

// ✅ Swagger UI (dev mode)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 🔥 Verify config loaded correctly
Console.WriteLine("🚀 CONFIG CHECK (from Program.cs):");
Console.WriteLine("OpenAI:ApiKey => " + builder.Configuration["OpenAI:ApiKey"]);
Console.WriteLine("OpenAI:ProjectId => " + builder.Configuration["OpenAI:ProjectId"]);


app.Run();
