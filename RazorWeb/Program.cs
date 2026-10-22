using Domain.Interfaces;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

// Знаходимо шлях до папки Data
var rootPath = builder.Environment.ContentRootPath;
var solutionPath = Path.GetFullPath(Path.Combine(rootPath, "../")); 
var dataFolder = Path.Combine(solutionPath, "Data");

if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);

var dbPath = Path.Combine(dataFolder, "shop.db");
var initSql = Path.Combine(dataFolder, "init.sql");
var seedSql = Path.Combine(dataFolder, "seed.sql");

if (!File.Exists(initSql) || !File.Exists(seedSql))
    throw new Exception($"Не знайдено SQL-файли у папці: {dataFolder}");

DbInitializer.EnsureCreated(dbPath, initSql, seedSql);

builder.Services.AddSingleton<ICategoryRepository>(_ => new SqliteCategoryRepository(dbPath));
builder.Services.AddSingleton<IResourceRepository>(_ => new SqliteResourceRepository(dbPath));
builder.Services.AddSingleton<IRequestRepository>(_ => new SqliteRequestRepository(dbPath));

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();