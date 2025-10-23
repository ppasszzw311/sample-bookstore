namespace BookStore.Models;

public class BookSearchOptions
{
    public Guid? CategoryId { get; set; }

    public string? Keyword { get; set; }

    public BookSortOption SortBy { get; set; } = BookSortOption.Newest;

    public bool MatchTag { get; set; }
}
