using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using BookStore.Models;
using Microsoft.AspNetCore.Hosting;

namespace BookStore.Data;

public class BookStoreRepository : IBookStoreRepository
{
    private readonly string _dataFile;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private BookStoreData _data;

    public BookStoreRepository(IWebHostEnvironment environment)
    {
        _dataFile = Path.Combine(environment.ContentRootPath, "App_Data", "store.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_dataFile)!);
        _data = LoadOrCreateData();
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _data.Categories
                .OrderBy(c => c.Name, StringComparer.Create(CultureInfo.CurrentCulture, ignoreCase: true))
                .ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Category?> GetCategoryAsync(Guid id)
    {
        await _mutex.WaitAsync();
        try
        {
            return _data.Categories.FirstOrDefault(c => c.Id == id);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task AddCategoryAsync(Category category)
    {
        await _mutex.WaitAsync();
        try
        {
            if (_data.Categories.Any(c => string.Equals(c.Name, category.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Category '{category.Name}' already exists.");
            }

            category.Id = Guid.NewGuid();
            _data.Categories.Add(category);
            await PersistAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> CategoryExistsAsync(string name)
    {
        await _mutex.WaitAsync();
        try
        {
            return _data.Categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<Book>> GetBooksAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _data.Books.ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<Book>> SearchBooksAsync(BookSearchOptions options)
    {
        await _mutex.WaitAsync();
        try
        {
            IEnumerable<Book> query = _data.Books;

            if (options.CategoryId.HasValue)
            {
                query = query.Where(book => book.CategoryId == options.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(options.Keyword))
            {
                var keyword = options.Keyword.Trim();
                query = query.Where(book =>
                    book.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    book.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(book.Description) && book.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (options.MatchTag && book.Tags.Any(tag => tag.Contains(keyword, StringComparison.OrdinalIgnoreCase))));
            }

            var cultureComparer = StringComparer.Create(CultureInfo.CurrentCulture, ignoreCase: true);

            query = options.SortBy switch
            {
                BookSortOption.Title => query.OrderBy(book => book.Title, cultureComparer),
                BookSortOption.Author => query.OrderBy(book => book.Author, cultureComparer),
                BookSortOption.PriceLowToHigh => query.OrderBy(book => book.Price),
                BookSortOption.PriceHighToLow => query.OrderByDescending(book => book.Price),
                BookSortOption.Popularity => query.OrderByDescending(book => book.ViewCount),
                _ => query.OrderByDescending(book => book.PublishedOn)
            };

            return query.ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Book?> GetBookAsync(Guid id)
    {
        await _mutex.WaitAsync();
        try
        {
            return _data.Books.FirstOrDefault(b => b.Id == id);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task AddBookAsync(Book book)
    {
        await _mutex.WaitAsync();
        try
        {
            book.Id = Guid.NewGuid();
            book.Tags ??= new List<string>();
            _data.Books.Add(book);
            await PersistAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task AddBooksAsync(IEnumerable<Book> books)
    {
        await _mutex.WaitAsync();
        try
        {
            foreach (var book in books)
            {
                book.Id = Guid.NewGuid();
                book.Tags ??= new List<string>();
                _data.Books.Add(book);
            }

            await PersistAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task IncrementViewCountAsync(Guid bookId)
    {
        await _mutex.WaitAsync();
        try
        {
            var book = _data.Books.FirstOrDefault(b => b.Id == bookId);
            if (book is null)
            {
                return;
            }

            book.ViewCount += 1;
            await PersistAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task AddSubscriptionAsync(Subscription subscription)
    {
        await _mutex.WaitAsync();
        try
        {
            if (_data.Subscriptions.Any(s => string.Equals(s.Email, subscription.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Subscription already exists for the provided email.");
            }

            subscription.Id = Guid.NewGuid();
            subscription.CreatedAt = DateTime.UtcNow;
            _data.Subscriptions.Add(subscription);
            await PersistAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> SubscriptionExistsAsync(string email)
    {
        await _mutex.WaitAsync();
        try
        {
            return _data.Subscriptions.Any(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _data.Subscriptions
                .OrderByDescending(s => s.CreatedAt)
                .ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<BookStatistics>> GetStatisticsAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            var categories = _data.Categories.ToDictionary(c => c.Id, c => c.Name);
            return _data.Books
                .GroupBy(book => book.CategoryId)
                .Select(group =>
                {
                    var prices = group.Select(book => book.Price).ToList();
                    return new BookStatistics
                    {
                        CategoryName = categories.TryGetValue(group.Key, out var name) ? name : "Unknown",
                        BookCount = group.Count(),
                        AveragePrice = prices.Any() ? Math.Round(prices.Average(), 2) : 0m,
                        HighestPrice = prices.Any() ? prices.Max() : 0m,
                        LowestPrice = prices.Any() ? prices.Min() : 0m
                    };
                })
                .OrderByDescending(stat => stat.BookCount)
                .ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    private BookStoreData LoadOrCreateData()
    {
        if (!File.Exists(_dataFile))
        {
            var data = CreateSeedData();
            EnsureCollections(data);
            File.WriteAllText(_dataFile, JsonSerializer.Serialize(data, _serializerOptions));
            return data;
        }

        using var stream = File.OpenRead(_dataFile);
        var loaded = JsonSerializer.Deserialize<BookStoreData>(stream, _serializerOptions) ?? new BookStoreData();
        EnsureCollections(loaded);
        return loaded;
    }

    private async Task PersistAsync()
    {
        EnsureCollections(_data);
        await using var stream = File.Create(_dataFile);
        await JsonSerializer.SerializeAsync(stream, _data, _serializerOptions);
    }

    private static void EnsureCollections(BookStoreData data)
    {
        data.Categories ??= new List<Category>();
        data.Books ??= new List<Book>();
        data.Subscriptions ??= new List<Subscription>();

        foreach (var book in data.Books)
        {
            book.Tags ??= new List<string>();
        }
    }

    private static BookStoreData CreateSeedData()
    {
        var fiction = new Category
        {
            Name = "小說",
            Description = "各類型故事小說"
        };

        var technology = new Category
        {
            Name = "科技",
            Description = "科技與程式設計書籍"
        };

        var design = new Category
        {
            Name = "設計",
            Description = "設計與創意靈感"
        };

        var business = new Category
        {
            Name = "商業",
            Description = "行銷、管理與創業"
        };

        var categories = new[] { fiction, technology, design, business };

        var books = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "未來城市的祕密",
                Author = "李安娜",
                Description = "一段關於永續都市與科技倫理的科幻冒險。",
                CategoryId = fiction.Id,
                PublishedOn = DateTime.UtcNow.Date.AddDays(-30),
                Price = 420,
                CoverImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400",
                ViewCount = 48,
                Tags = new() { "科幻", "永續", "冒險" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "C# 開發實戰",
                Author = "王信哲",
                Description = "從基礎到進階的 C# 與 .NET 8 開發指南。",
                CategoryId = technology.Id,
                PublishedOn = DateTime.UtcNow.Date.AddDays(-12),
                Price = 680,
                CoverImageUrl = "https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?w=400",
                ViewCount = 133,
                Tags = new() { "程式設計", "C#", ".NET" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "以使用者為中心的設計",
                Author = "林知音",
                Description = "實用的 UX 流程與設計工具介紹。",
                CategoryId = design.Id,
                PublishedOn = DateTime.UtcNow.Date.AddDays(-5),
                Price = 560,
                CoverImageUrl = "https://images.unsplash.com/photo-1491841550275-ad7854e35ca6?w=400",
                ViewCount = 78,
                Tags = new() { "UX", "UI", "產品設計" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "敏捷團隊的高效協作",
                Author = "吳家豪",
                Description = "打造高效軟體團隊的敏捷管理實務。",
                CategoryId = business.Id,
                PublishedOn = DateTime.UtcNow.Date.AddDays(-50),
                Price = 520,
                CoverImageUrl = "https://images.unsplash.com/photo-1473755504818-b72b6dfdc226?w=400",
                ViewCount = 92,
                Tags = new() { "敏捷", "管理", "軟體" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "資料視覺化思維",
                Author = "陳怡婷",
                Description = "從資料到故事的視覺化策略。",
                CategoryId = design.Id,
                PublishedOn = DateTime.UtcNow.Date.AddDays(-2),
                Price = 490,
                CoverImageUrl = "https://images.unsplash.com/photo-1528459105426-b1c4f1996c5d?w=400",
                ViewCount = 63,
                Tags = new() { "資料", "視覺化", "策略" }
            }
        };

        return new BookStoreData
        {
            Categories = categories.ToList(),
            Books = books,
            Subscriptions = new List<Subscription>()
        };
    }
}
