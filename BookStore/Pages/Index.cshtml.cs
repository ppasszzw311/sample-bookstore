using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace BookStore.Pages;

public class IndexModel : PageModel
{
    private readonly IBookStoreRepository _repository;

    public IndexModel(IBookStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Book> NewBooks { get; private set; } = Array.Empty<Book>();

    public IReadOnlyList<Book> PopularBooks { get; private set; } = Array.Empty<Book>();

    [BindProperty]
    public string SubscriptionEmail { get; set; } = string.Empty;

    public string? SubscriptionMessage { get; private set; }

    public bool SubscriptionSuccess { get; private set; }

    public async Task OnGetAsync()
    {
        var books = await _repository.GetBooksAsync();
        NewBooks = books
            .OrderByDescending(book => book.PublishedOn)
            .Take(4)
            .ToList();

        PopularBooks = books
            .OrderByDescending(book => book.ViewCount)
            .ThenByDescending(book => book.PublishedOn)
            .Take(5)
            .ToList();
    }

    public async Task<IActionResult> OnPostSubscribeAsync()
    {
        await OnGetAsync();

        if (string.IsNullOrWhiteSpace(SubscriptionEmail))
        {
            ModelState.AddModelError(nameof(SubscriptionEmail), "請輸入 email。");
            return Page();
        }

        if (!SubscriptionEmail.Contains('@'))
        {
            ModelState.AddModelError(nameof(SubscriptionEmail), "Email 格式不正確。");
            return Page();
        }

        if (await _repository.SubscriptionExistsAsync(SubscriptionEmail))
        {
            SubscriptionMessage = "您已訂閱最新書訊，我們將持續寄送最新資訊給您。";
            return Page();
        }

        await _repository.AddSubscriptionAsync(new Subscription { Email = SubscriptionEmail });
        SubscriptionSuccess = true;
        SubscriptionMessage = "感謝訂閱！請留意您的信箱，第一手掌握新書資訊。";
        ModelState.Clear();
        SubscriptionEmail = string.Empty;

        return Page();
    }
}
