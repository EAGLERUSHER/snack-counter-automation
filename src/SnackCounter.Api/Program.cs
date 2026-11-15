using System.Text.Json.Serialization;
using SnackCounter.Api.Middleware;
using SnackCounter.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SnackCounterApplication>();
builder.Services.AddSingleton<AuthSessionStore>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("AngularDev");
app.UseMiddleware<EmployeeAuthenticationMiddleware>();

var wwwroot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var hasUi = Directory.Exists(wwwroot) && File.Exists(Path.Combine(wwwroot, "index.html"));
if (hasUi)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.MapControllers();

if (hasUi)
{
    app.MapFallbackToFile("index.html");
}

app.Run();