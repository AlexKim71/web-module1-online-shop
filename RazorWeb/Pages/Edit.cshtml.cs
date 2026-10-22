using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWeb.Pages;

public class EditModel : PageModel
{
    private readonly IResourceRepository _resources;
    private readonly ICategoryRepository _categories;

    public EditModel(IResourceRepository resources, ICategoryRepository categories)
    {
        _resources = resources;
        _categories = categories;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Categories { get; private set; } = default!;

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Введіть код")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Код від 3 до 20 символів")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Введіть назву")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Назва від 2 до 120 символів")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Введіть ціну")]
        public string PriceUah { get; set; } = "";

        [Required(ErrorMessage = "Введіть SKU")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "SKU від 3 до 30 символів")]
        public string Sku { get; set; } = "";

        [Required(ErrorMessage = "Введіть бренд")]
        [StringLength(60, MinimumLength = 1, ErrorMessage = "Бренд від 1 до 60 символів")]
        public string Brand { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var resource = await _resources.GetByIdAsync(id);
        if (resource == null) return NotFound();

        Input = new InputModel
        {
            Id = resource.Id,
            CategoryId = resource.CategoryId,
            Code = resource.Code,
            Name = resource.Name,
            PriceUah = (resource.PriceMinor / 100.0m).ToString("0.00"),
            Sku = resource.Sku,
            Brand = resource.Brand
        };

        await LoadCategories();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategories();
        if (!ModelState.IsValid) return Page();

        if (!DomainRules.TryParseUahToMinor(Input.PriceUah, out var minor))
        {
            ModelState.AddModelError("Input.PriceUah", "Формат ціни: 19,99 (не більше 2 знаків)");
            return Page();
        }

        var resource = await _resources.GetByIdAsync(Input.Id);
        if (resource == null) return NotFound();

        resource.CategoryId = Input.CategoryId;
        resource.Code = Input.Code.Trim();
        resource.Name = Input.Name.Trim();
        resource.PriceMinor = minor;
        resource.Sku = Input.Sku.Trim();
        resource.Brand = Input.Brand.Trim();

        try
        {
            await _resources.UpdateAsync(resource);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }

        return RedirectToPage("Index");
    }

    private async Task LoadCategories()
    {
        Categories = new SelectList(await _categories.GetAllAsync(), "Id", "Name");
    }
}