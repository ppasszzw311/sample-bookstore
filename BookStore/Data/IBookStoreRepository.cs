using BookStore.Models;

namespace BookStore.Data;

public interface IBookStoreRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync();

    Task<Category?> GetCategoryAsync(Guid id);

    Task AddCategoryAsync(Category category);

    Task<bool> CategoryExistsAsync(string name);

    Task<IReadOnlyList<Book>> GetBooksAsync();

    Task<IReadOnlyList<Book>> SearchBooksAsync(BookSearchOptions options);

    Task<Book?> GetBookAsync(Guid id);

    Task AddBookAsync(Book book);

    Task AddBooksAsync(IEnumerable<Book> books);

    Task IncrementViewCountAsync(Guid bookId);

    Task AddSubscriptionAsync(Subscription subscription);

    Task<bool> SubscriptionExistsAsync(string email);

    Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync();

    Task<IReadOnlyList<BookStatistics>> GetStatisticsAsync();
}
