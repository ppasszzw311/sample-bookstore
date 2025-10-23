using BookStore.Models;

namespace BookStore.Data;

public class BookStoreData
{
    public List<Category> Categories { get; set; } = new();

    public List<Book> Books { get; set; } = new();

    public List<Subscription> Subscriptions { get; set; } = new();
}
