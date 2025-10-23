using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using BookStore.Data;
using BookStore.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Pages.Admin.Books;

public class IndexModel : PageModel
{
    private readonly IBookStoreRepository _repository;

    public IndexModel(IBookStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Book> Books { get; private set; } = Array.Empty<Book>();

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();

    [BindProperty]
    public BookInputModel Input { get; set; } = new();

    [BindProperty]
    public string? BatchContent { get; set; }

    public string? StatusMessage { get; private set; }

    public string? BatchStatusMessage { get; private set; }

    public List<string> BatchErrors { get; } = new();

    public SelectList CategorySelectList => new(Categories, nameof(Category.Id), nameof(Category.Name));

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
        Input.PublishedOn = DateTime.UtcNow.Date;
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        await LoadDataAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.CategoryId == Guid.Empty)
        {
            ModelState.AddModelError("Input.CategoryId", "請選擇分類。");
            return Page();
        }

        var book = new Book
        {
            Title = Input.Title,
            Author = Input.Author,
            Description = Input.Description,
            CategoryId = Input.CategoryId,
            PublishedOn = Input.PublishedOn,
            Price = Input.Price,
            CoverImageUrl = Input.CoverImageUrl,
            Tags = ParseTags(Input.Tags)
        };

        await _repository.AddBookAsync(book);
        StatusMessage = $"已新增書目：{book.Title}";
        ModelState.Clear();
        Input = new BookInputModel { PublishedOn = DateTime.UtcNow.Date };
        Books = await _repository.GetBooksAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostBatchAsync()
    {
        await LoadDataAsync();

        if (string.IsNullOrWhiteSpace(BatchContent))
        {
            BatchErrors.Add("請貼上要匯入的 CSV 內容。");
            return Page();
        }

        using var reader = new StringReader(BatchContent);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
        csv.Context.MissingFieldFound = null;
        csv.Context.HeaderValidated = null;

        var records = new List<Book>();
        var lineNumber = 1;
        try
        {
            var categoryLookup = Categories.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);
            await foreach (var record in csv.GetRecordsAsync<BatchBookRecord>())
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(record.Title) || string.IsNullOrWhiteSpace(record.Author))
                {
                    BatchErrors.Add($"第 {lineNumber} 行缺少書名或作者。");
                    continue;
                }

                if (!categoryLookup.TryGetValue(record.Category, out var categoryId))
                {
                    BatchErrors.Add($"第 {lineNumber} 行的分類 '{record.Category}' 不存在。");
                    continue;
                }

                if (!DateTime.TryParse(record.PublishedOn, CultureInfo.CurrentCulture, DateTimeStyles.None, out var publishedOn))
                {
                    BatchErrors.Add($"第 {lineNumber} 行的出版日期格式不正確。");
                    continue;
                }

                if (!decimal.TryParse(record.Price, NumberStyles.Number, CultureInfo.CurrentCulture, out var price))
                {
                    BatchErrors.Add($"第 {lineNumber} 行的價格格式不正確。");
                    continue;
                }

                records.Add(new Book
                {
                    Title = record.Title!,
                    Author = record.Author!,
                    CategoryId = categoryId,
                    Description = record.Description,
                    PublishedOn = publishedOn,
                    Price = price,
                    CoverImageUrl = record.CoverImageUrl,
                    Tags = ParseTags(record.Tags)
                });
            }
        }
        catch (Exception ex)
        {
            BatchErrors.Add($"匯入時發生錯誤：{ex.Message}");
        }

        if (records.Any())
        {
            await _repository.AddBooksAsync(records);
            BatchStatusMessage = $"成功匯入 {records.Count} 本書。";
            if (!BatchErrors.Any())
            {
                BatchContent = string.Empty;
                ModelState.Remove(nameof(BatchContent));
            }
        }
        else
        {
            BatchErrors.Add("沒有任何有效的書目可以匯入。");
        }

        Books = await _repository.GetBooksAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        Categories = await _repository.GetCategoriesAsync();
        Books = await _repository.GetBooksAsync();
    }

    private static List<string> ParseTags(string? tags)
    {
        return string.IsNullOrWhiteSpace(tags)
            ? new List<string>()
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public class BookInputModel
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime PublishedOn { get; set; } = DateTime.UtcNow.Date;

        [Range(0, 9999)]
        public decimal Price { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? Tags { get; set; }
    }

    private class BatchBookRecord
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PublishedOn { get; set; }
        public string? Price { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Tags { get; set; }
    }
}
