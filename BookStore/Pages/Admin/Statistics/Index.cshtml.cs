using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Statistics;

public class IndexModel : PageModel
{
    private readonly IBookStoreRepository _repository;

    public IndexModel(IBookStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<BookStatistics> Statistics { get; private set; } = Array.Empty<BookStatistics>();

    public IReadOnlyList<Book> Books { get; private set; } = Array.Empty<Book>();

    public async Task OnGetAsync()
    {
        Statistics = await _repository.GetStatisticsAsync();
        Books = await _repository.GetBooksAsync();
    }
}
