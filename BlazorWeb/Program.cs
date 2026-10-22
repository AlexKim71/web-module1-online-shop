using Domain.Interfaces;
using Infrastructure;
using BlazorWeb.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Знаходимо шлях до БД (як у RazorWeb)
var rootPath = builder.Environment.ContentRootPath;
var solutionPath = Path.GetFullPath(Path.Combine(rootPath, "../"));
var dataFolder = Path.Combine(solutionPath, "Data");
var dbPath = Path.Combine(dataFolder, "shop.db");

// Реєструємо репозиторії
builder.Services.AddSingleton<ICategoryRepository>(_ => new SqliteCategoryRepository(dbPath));
builder.Services.AddSingleton<IResourceRepository>(_ => new SqliteResourceRepository(dbPath));

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();