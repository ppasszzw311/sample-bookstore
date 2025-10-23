using BookStore.Data;
using BookStore.Models;
using System.Linq;

namespace BookStore.Services;

public class BookRecommendationService : IBookRecommendationService
{
    private readonly IBookStoreRepository _repository;

    public BookRecommendationService(IBookStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Book>> GetRelatedBooksAsync(Book book, int limit = 4)
    {
        var books = await _repository.GetBooksAsync();
        return books
            .Where(candidate => candidate.Id != book.Id && candidate.CategoryId == book.CategoryId)
            .OrderByDescending(candidate => candidate.Tags.Intersect(book.Tags, StringComparer.OrdinalIgnoreCase).Count())
            .ThenByDescending(candidate => candidate.ViewCount)
            .ThenByDescending(candidate => candidate.PublishedOn)
            .Take(limit)
            .ToList();
    }
}
