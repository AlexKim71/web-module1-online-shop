using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Xml.Linq;

namespace RazorWeb.Pages.Xml;

public class ExportModel : PageModel
{
    private readonly IResourceRepository _resources;
    private readonly ICategoryRepository _categories;

    public ExportModel(IResourceRepository resources, ICategoryRepository categories)
    {
        _resources = resources;
        _categories = categories;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var cats = await _categories.GetAllAsync();
            var res = await _resources.GetAllAsync();

            var xdoc = new XDocument(
                new XElement("catalog", new XAttribute("schemaVersion", "1"),
                    new XElement("categories",
                        from c in cats
                        select new XElement("category", new XAttribute("id", c.Id), new XAttribute("name", c.Name))
                    ),
                    new XElement("resources",
                        from r in res
                        select new XElement("resource", 
                            new XAttribute("id", r.Id), 
                            new XAttribute("categoryId", r.CategoryId), 
                            new XAttribute("code", r.Code),
                            new XElement("name", r.Name),
                            new XElement("priceMinor", r.PriceMinor),
                            new XElement("capacity", r.Capacity),
                            new XElement("sku", r.Sku),
                            new XElement("brand", r.Brand)
                        )
                    )
                )
            );

            var xmlString = xdoc.ToString();
            var bytes = Encoding.UTF8.GetBytes(xmlString);
            
            return File(bytes, "application/xml", "catalog-export.xml");
        }
        catch (Exception ex)
        {
            // Якщо станеться помилка, ми побачимо її на екрані
            return Content($"Критична помилка при експорті: {ex.Message}");
        }
    }
}