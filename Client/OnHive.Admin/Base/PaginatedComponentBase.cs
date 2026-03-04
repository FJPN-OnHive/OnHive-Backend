using OnHive.Admin.Helpers;
using OnHive.Core.Library.Contracts.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OnHive.Admin.Base;

public abstract class PaginatedComponentBase<TEntity> : ComponentBase where TEntity : class
{
    protected int TotalPages { get; set; }
    protected RequestFilter Filter { get; set; } = new() { Page = 1, PageLimit = 10 };
    protected bool IsLoading { get; set; }
    protected string SearchTerm { get; set; } = string.Empty;

    protected void CalculateTotalPages(long pageCount, long total, int returnedCount) =>
        TotalPages = PaginationHelper.CalculateTotalPages(pageCount, total, Filter.PageLimit, returnedCount);

    protected virtual async Task PageChanged(int page)
    {
        var validatedPage = PaginationHelper.ValidatePageNumber(page, TotalPages);

        if (Filter.Page != validatedPage)
        {
            Filter.Page = validatedPage;
            await LoadDataWithLoadingAsync();
        }
    }

    protected virtual async Task ChangePageLimit(int pageSize)
    {
        if (Filter.PageLimit != pageSize)
        {
            Filter.PageLimit = pageSize;
            Filter.Page = 1;
            await LoadDataWithLoadingAsync();
        }
    }

    protected virtual async Task Search()
    {
        Filter.Page = 1;
        await LoadDataWithLoadingAsync();
    }

    private async Task LoadDataWithLoadingAsync()
    {
        try
        {
            IsLoading = true;
            await LoadDataAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ApplySearchFilter(params string[] searchFields) =>
        PaginationHelper.ApplyOrSearchFilter(Filter, SearchTerm, searchFields);

    protected abstract Task LoadDataAsync();

    protected Task GoToFirstPage() => PageChanged(1);

    protected Task GoToPreviousPage() =>
        Filter.Page > 1 ? PageChanged(Filter.Page - 1) : Task.CompletedTask;

    protected Task GoToNextPage() =>
        Filter.Page < TotalPages ? PageChanged(Filter.Page + 1) : Task.CompletedTask;

    protected Task GoToLastPage() => PageChanged(TotalPages);

    protected async Task OnSearchKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
            await Search();
    }
}
