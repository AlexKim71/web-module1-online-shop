using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWeb.Pages.Requests;

public class CreateModel : PageModel
{
    private readonly IRequestRepository _requests;
    private readonly IResourceRepository _resources;

    public CreateModel(IRequestRepository requests, IResourceRepository resources)
    {
        _requests = requests;
        _resources = resources;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ResourcesList { get; private set; } = default!;

    public class InputModel
    {
        [Required(ErrorMessage = "Оберіть товар")]
        public int ResourceId { get; set; }

        [Required(ErrorMessage = "Введіть ім'я")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Ім'я від 2 до 80 символів")]
        public string CustomerName { get; set; } = "";

        [Range(1, 100000, ErrorMessage = "Кількість від 1 до 100 000")]
        public int Quantity { get; set; } = 1;
    }

    public async Task OnGetAsync() => await LoadResources();

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadResources();
        if (!ModelState.IsValid) return Page();

        try
        {
            await _requests.CreateAsync(Input.ResourceId, Input.CustomerName, Input.Quantity);
        }
        catch (Exception ex)
        {
            // Якщо транзакція відкотилася (наприклад, недостатньо товару)
            ModelState.AddModelError("", ex.Message);
            return Page();
        }

        return RedirectToPage("Index");
    }

    private async Task LoadResources()
    {
        var all = await _resources.GetAllAsync();
        ResourcesList = new SelectList(all, "Id", "Name");
    }
}