namespace BookStore.Models;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public DateTime PublishedOn { get; set; }

    public decimal Price { get; set; }

    public string? CoverImageUrl { get; set; }

    public int ViewCount { get; set; }

    public List<string> Tags { get; set; } = new();
}
