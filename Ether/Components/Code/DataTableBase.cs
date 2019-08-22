using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Types;
using Ether.Types.Exceptions;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Code
{
    public abstract class DataTableBase<TItem> : ComponentBase
    {
        private Func<TItem, bool> _filterPredicate;

        protected TItem ItemWithDetailsVisible { get; set; } = default;

        protected IEnumerable<TItem> ItemsToShow { get; private set; } = Enumerable.Empty<TItem>();

        [Parameter]
        protected int CurrentPage { get; set; }

        [Parameter]
        protected int TotalPages { get; set; }

        [Parameter] protected RenderFragment TableHeader { get; set; }
        [Parameter] protected RenderFragment<TItem> RowTemplate { get; set; }
        [Parameter] protected RenderFragment<TItem> RowDetailTemplate { get; set; }
        [Parameter] protected RenderFragment<TItem> ActionsTemplate { get; set; }
        [Parameter] protected RenderFragment TableFooter { get; set; }
        [Parameter] protected RenderFragment<DataTableBase<TItem>> FilterTemplate { get; set; }
        [Parameter] protected IEnumerable<TItem> Items { get; set; }
        [Parameter] protected bool IsServerPaging { get; set; }
        [Parameter] protected int PageSize { get; set; } = 10;
        [Parameter] protected string NoItemsText { get; set; }
        [Parameter] protected Func<TItem, string> RowClass { get; set; }
        [Parameter] protected OrderByConfiguration<TItem>[] OrderBy { get; set; }
        [Parameter] protected EventCallback OnNextPage { get; set; }
        [Parameter] protected EventCallback OnPreviousPage { get; set; }

        public void Filter(Func<TItem, bool> predicate)
        {
            CurrentPage = 1;
            _filterPredicate = predicate;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (IsServerPaging && (!OnNextPage.HasDelegate || !OnPreviousPage.HasDelegate))
            {
                throw new ArgumentException("When server paging is enabled OnNextPage and OnPreviousPage have to be bound.");
            }

            if (!IsServerPaging)
            {
                CurrentPage = 1;
            }

            ItemsToShow = Items;

            // TODO: Filtering and sorting needs to be server side
            if (_filterPredicate != null)
            {
                ItemsToShow = ItemsToShow.Where(_filterPredicate);
            }

            if (OrderBy != null && OrderBy.Any())
            {
                foreach (var orderConfig in OrderBy)
                {
                    ItemsToShow = orderConfig.IsDescending ? ItemsToShow.OrderByDescending(orderConfig.Property) : ItemsToShow.OrderBy(orderConfig.Property);
                }
            }

            if (!IsServerPaging)
            {
                TotalPages = (int)Math.Ceiling(Items.Count() / (decimal)PageSize);
            }
        }

        protected async Task NextPage()
        {
            if (!IsServerPaging)
            {
                CurrentPage++;
            }
            else
            {
                await OnNextPage.InvokeAsync(this);
            }

            Console.WriteLine("Going to next page");

            StateHasChanged();
        }

        protected async Task PrevPage()
        {
            if (!IsServerPaging)
            {
                CurrentPage--;
            }
            else
            {
                await OnPreviousPage.InvokeAsync(this);
            }

            Console.WriteLine("Going to prev page");

            StateHasChanged();
        }

        protected string GetNoItemsText()
        {
            return string.IsNullOrEmpty(NoItemsText) ? "No items" : NoItemsText;
        }

        protected void ToggleDetails(TItem item)
        {
            ItemWithDetailsVisible = IsItemDetailsVisible(item) ? default : item;
        }

        protected bool IsItemDetailsVisible(TItem item)
        {
            return Equals(ItemWithDetailsVisible, item);
        }
    }
}
