using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWeb.Pages;

public class CreateModel : PageModel
{
    private readonly IResourceRepository _resources;
    private readonly ICategoryRepository _categories;

    public CreateModel(IResourceRepository resources, ICategoryRepository categories)
    {
        _resources = resources;
        _categories = categories;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Categories { get; private set; } = default!;

    public class InputModel
    {
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

        [Range(0, 100000, ErrorMessage = "Від 0 до 100 000")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Введіть SKU")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "SKU від 3 до 30 символів")]
        public string Sku { get; set; } = "";

        [Required(ErrorMessage = "Введіть бренд")]
        [StringLength(60, MinimumLength = 1, ErrorMessage = "Бренд від 1 до 60 символів")]
        public string Brand { get; set; } = "";
    }

    public async Task OnGetAsync() => await LoadCategories();

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategories();
        if (!ModelState.IsValid) return Page();

        if (!DomainRules.TryParseUahToMinor(Input.PriceUah, out var minor))
        {
            ModelState.AddModelError("Input.PriceUah", "Формат ціни: 19,99 (не більше 2 знаків)");
            return Page();
        }

        var resource = new Resource
        {
            CategoryId = Input.CategoryId,
            Code = Input.Code.Trim(),
            Name = Input.Name.Trim(),
            PriceMinor = minor,
            Capacity = Input.Capacity,
            Available = Input.Capacity,
            Sku = Input.Sku.Trim(),
            Brand = Input.Brand.Trim()
        };

        try
        {
            await _resources.AddAsync(resource);
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