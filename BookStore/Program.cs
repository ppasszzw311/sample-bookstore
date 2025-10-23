using BookStore.Data;
using BookStore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<BookStoreRepository>();
builder.Services.AddSingleton<IBookStoreRepository>(sp => sp.GetRequiredService<BookStoreRepository>());
builder.Services.AddSingleton<IBookRecommendationService, BookRecommendationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
