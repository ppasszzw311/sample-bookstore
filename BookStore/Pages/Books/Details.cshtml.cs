using BookStore.Data;
using BookStore.Models;
using BookStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Books;

public class DetailsModel : PageModel
{
    private readonly IBookStoreRepository _repository;
    private readonly IBookRecommendationService _recommendationService;

    public DetailsModel(IBookStoreRepository repository, IBookRecommendationService recommendationService)
    {
        _repository = repository;
        _recommendationService = recommendationService;
    }

    public Book? Book { get; private set; }

    public Category? Category { get; private set; }

    public IReadOnlyList<Book> RelatedBooks { get; private set; } = Array.Empty<Book>();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var book = await _repository.GetBookAsync(id);
        if (book is null)
        {
            return NotFound();
        }

        await _repository.IncrementViewCountAsync(book.Id);
        Book = await _repository.GetBookAsync(id);
        Category = await _repository.GetCategoryAsync(Book!.CategoryId);
        RelatedBooks = await _recommendationService.GetRelatedBooksAsync(Book);

        return Page();
    }
}
