var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapReverseProxy();
app.MapHealthChecks("/health");

app.Run();
