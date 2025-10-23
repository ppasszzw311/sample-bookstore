using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Categories;

public class IndexModel : PageModel
{
    private readonly IBookStoreRepository _repository;

    public IndexModel(IBookStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();

    [BindProperty]
    public Category Input { get; set; } = new();

    public string? StatusMessage { get; private set; }

    public async Task OnGetAsync()
    {
        Categories = await _repository.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await _repository.GetCategoriesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Input.Name))
        {
            ModelState.AddModelError("Input.Name", "請輸入分類名稱。");
            return Page();
        }

        if (await _repository.CategoryExistsAsync(Input.Name))
        {
            ModelState.AddModelError("Input.Name", "分類名稱重複。請使用其他名稱。");
            return Page();
        }

        await _repository.AddCategoryAsync(new Category
        {
            Name = Input.Name,
            Description = Input.Description
        });

        StatusMessage = $"已新增分類：{Input.Name}";
        ModelState.Clear();
        Input = new Category();
        Categories = await _repository.GetCategoriesAsync();

        return Page();
    }
}
