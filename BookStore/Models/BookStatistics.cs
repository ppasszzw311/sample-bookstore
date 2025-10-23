namespace BookStore.Models;

public class BookStatistics
{
    public string CategoryName { get; set; } = string.Empty;

    public int BookCount { get; set; }

    public decimal AveragePrice { get; set; }

    public decimal HighestPrice { get; set; }

    public decimal LowestPrice { get; set; }
}
