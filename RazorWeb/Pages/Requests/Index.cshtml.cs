using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorWeb.Pages.Requests;

public class IndexModel : PageModel
{
    private readonly IRequestRepository _requests;

    public IndexModel(IRequestRepository requests)
    {
        _requests = requests;
    }

    public IReadOnlyList<Request> RequestsList { get; set; } = new List<Request>();

    public async Task OnGetAsync()
    {
        RequestsList = await _requests.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        await _requests.CancelAsync(id);
        return RedirectToPage("Index");
    }
}