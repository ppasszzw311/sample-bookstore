using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace BookStore.Pages.Books;

public class IndexModel : PageModel
{
    private readonly IBookStoreRepository _repository;

    public IndexModel(IBookStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();

    public IReadOnlyList<Book> Books { get; private set; } = Array.Empty<Book>();

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public BookSortOption SortBy { get; set; } = BookSortOption.Newest;

    [BindProperty(SupportsGet = true)]
    public bool MatchTag { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _repository.GetCategoriesAsync();
        Books = await _repository.SearchBooksAsync(new BookSearchOptions
        {
            CategoryId = CategoryId,
            Keyword = Keyword,
            SortBy = SortBy,
            MatchTag = MatchTag
        });
    }
}
