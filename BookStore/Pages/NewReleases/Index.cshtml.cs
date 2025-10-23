using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace BookStore.Pages.NewReleases;

public class IndexModel : PageModel
{
    private readonly IBookStoreRepository _repository;

    public IndexModel(IBookStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Book> Books { get; private set; } = Array.Empty<Book>();

    [BindProperty(SupportsGet = true)]
    public int Days { get; set; } = 30;

    public IReadOnlyList<int> DayOptions { get; } = new[] { 7, 30, 90 };

    public async Task OnGetAsync()
    {
        var books = await _repository.GetBooksAsync();
        var threshold = DateTime.UtcNow.Date.AddDays(-Days);
        Books = books
            .Where(book => book.PublishedOn.Date >= threshold)
            .OrderByDescending(book => book.PublishedOn)
            .ToList();
    }
}
