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
        public int CurrentPage { get; set; }

        [Parameter]
        public int TotalPages { get; set; }

        [Parameter] public RenderFragment TableHeader { get; set; }
        [Parameter] public RenderFragment<TItem> RowTemplate { get; set; }
        [Parameter] public RenderFragment<TItem> RowDetailTemplate { get; set; }
        [Parameter] public RenderFragment<TItem> ActionsTemplate { get; set; }
        [Parameter] public RenderFragment TableFooter { get; set; }
        [Parameter] public RenderFragment<DataTableBase<TItem>> FilterTemplate { get; set; }
        [Parameter] public IEnumerable<TItem> Items { get; set; }
        [Parameter] public bool IsServerPaging { get; set; }
        [Parameter] public int PageSize { get; set; } = 10;
        [Parameter] public string NoItemsText { get; set; }
        [Parameter] public Func<TItem, string> RowClass { get; set; }
        [Parameter] public OrderByConfiguration<TItem>[] OrderBy { get; set; }
        [Parameter] public EventCallback OnNextPage { get; set; }
        [Parameter] public EventCallback OnPreviousPage { get; set; }

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
