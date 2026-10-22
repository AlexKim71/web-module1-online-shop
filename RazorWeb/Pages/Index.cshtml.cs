using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorWeb.Pages;

public class IndexModel : PageModel
{
    private readonly IResourceRepository _resources;

    public IndexModel(IResourceRepository resources)
    {
        _resources = resources;
    }

    public IReadOnlyList<Resource> ResourcesList { get; set; } = new List<Resource>();

    public async Task OnGetAsync()
    {
        ResourcesList = await _resources.GetAllAsync();
    }
}