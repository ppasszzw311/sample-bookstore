using BookStore.Models;

namespace BookStore.Services;

public interface IBookRecommendationService
{
    Task<IReadOnlyList<Book>> GetRelatedBooksAsync(Book book, int limit = 4);
}
