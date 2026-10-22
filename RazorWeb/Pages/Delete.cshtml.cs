using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorWeb.Pages;

public class DeleteModel : PageModel
{
    private readonly IResourceRepository _resources;

    public DeleteModel(IResourceRepository resources)
    {
        _resources = resources;
    }

    public Resource? Resource { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Resource = await _resources.GetByIdAsync(id);
        if (Resource == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        // Репозиторій повертає false, якщо на ресурс є заявки (навіть скасовані)
        var deleted = await _resources.DeleteAsync(id);
        
        if (deleted)
        {
            return RedirectToPage("Index");
        }
        
        // Якщо видалити не вдалося (є заявки), повертаємо сторінку з помилкою
        Resource = await _resources.GetByIdAsync(id);
        ErrorMessage = "Неможливо видалити товар, оскільки на нього вже створено заявки!";
        return Page();
    }
}