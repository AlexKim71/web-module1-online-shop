using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.Data.Sqlite;

namespace RazorWeb.Pages.Xml;

public class ImportModel : PageModel
{
    private readonly string _dbPath;
    private readonly string _xsdPath;

    public ImportModel(IWebHostEnvironment env)
    {
        // Шукаємо папку WebModule1 (піднімаємось з RazorWeb на один рівень вище)
        var solutionPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "../"));
        _dbPath = Path.Combine(solutionPath, "Data", "shop.db");
        _xsdPath = Path.Combine(solutionPath, "Xml", "catalog.xsd");
    }

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    public string? Message { get; set; }
    public bool IsError { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            Message = "Файл не вибрано.";
            IsError = true;
            return Page();
        }

        if (!System.IO.File.Exists(_xsdPath))
        {
            Message = $"Критична помилка: Не знайдено XSD-схему за шляхом {_xsdPath}";
            IsError = true;
            return Page();
        }

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                ValidationType = ValidationType.Schema
            };
            settings.Schemas.Add(null, _xsdPath);
            
            // Збираємо помилки валідації
            var errors = new List<string>();
            settings.ValidationEventHandler += (s, e) => errors.Add(e.Message);

            using var stream = UploadedFile.OpenReadStream();
            using var reader = XmlReader.Create(stream, settings);
            var xdoc = XDocument.Load(reader);

            if (errors.Any())
            {
                Message = "Помилка валідації XSD: " + string.Join("; ", errors);
                IsError = true;
                return Page();
            }

            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            await conn.OpenAsync();
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Resources";
            if (Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0)
            {
                Message = "Помилка: База даних не є порожньою. Імпорт дозволено лише в порожню БД.";
                IsError = true;
                return Page();
            }

            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var c in xdoc.Descendants("category"))
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = "INSERT INTO Categories (Id, Name) VALUES ($id, $name)";
                    cmd.Parameters.AddWithValue("$id", int.Parse(c.Attribute("id")!.Value));
                    cmd.Parameters.AddWithValue("$name", c.Attribute("name")!.Value);
                    await cmd.ExecuteNonQueryAsync();
                }

                foreach (var r in xdoc.Descendants("resource"))
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT INTO Resources (Id, CategoryId, Code, Name, PriceMinor, Capacity, Available, Sku, Brand) 
                                        VALUES ($id, $catId, $code, $name, $price, $cap, $cap, $sku, $brand)";
                    cmd.Parameters.AddWithValue("$id", int.Parse(r.Attribute("id")!.Value));
                    cmd.Parameters.AddWithValue("$catId", int.Parse(r.Attribute("categoryId")!.Value));
                    cmd.Parameters.AddWithValue("$code", r.Attribute("code")!.Value);
                    cmd.Parameters.AddWithValue("$name", r.Element("name")!.Value);
                    cmd.Parameters.AddWithValue("$price", long.Parse(r.Element("priceMinor")!.Value));
                    cmd.Parameters.AddWithValue("$cap", int.Parse(r.Element("capacity")!.Value));
                    cmd.Parameters.AddWithValue("$sku", r.Element("sku")!.Value);
                    cmd.Parameters.AddWithValue("$brand", r.Element("brand")!.Value);
                    await cmd.ExecuteNonQueryAsync();
                }

                tx.Commit();
                Message = "Успіх! Каталог успішно імпортовано.";
                IsError = false;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Message = $"Помилка імпорту: {ex.Message}";
            IsError = true;
        }

        return Page();
    }
}