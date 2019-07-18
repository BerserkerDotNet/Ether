using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Types;
using Ether.Types.Exceptions;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Code
{
    public abstract class DataTableBase<TItem> : ComponentBase
    {
        private Func<TItem, bool> _filterPredicate;

        protected int CurrentPage { get; private set; } = 1;

        protected int TotalPages { get; private set; } = 0;

        protected int ItemsCount { get; private set; } = 0;

        protected TItem ItemWithDetailsVisible { get; set; } = default;

        protected IEnumerable<TItem> ItemsToShow { get; private set; } = Enumerable.Empty<TItem>();

        [Parameter] protected RenderFragment TableHeader { get; set; }
        [Parameter] protected RenderFragment<TItem> RowTemplate { get; set; }
        [Parameter] protected RenderFragment<TItem> RowDetailTemplate { get; set; }
        [Parameter] protected RenderFragment<TItem> ActionsTemplate { get; set; }
        [Parameter] protected RenderFragment TableFooter { get; set; }
        [Parameter] protected RenderFragment<DataTableBase<TItem>> FilterTemplate { get; set; }
        [Parameter] protected IEnumerable<TItem> Items { get; set; }
        [Parameter] protected int PageSize { get; set; } = 10;
        [Parameter] protected string NoItemsText { get; set; }
        [Parameter] protected Func<TItem, string> RowClass { get; set; }
        [Parameter] protected OrderByConfiguration<TItem>[] OrderBy { get; set; }

        public void Filter(Func<TItem, bool> predicate)
        {
            CurrentPage = 1;
            _filterPredicate = predicate;
        }

        protected override void OnParametersSet()
        {
            Console.WriteLine("DT: Parameters set");
            base.OnParametersSet();
            ItemsToShow = Items;
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

            ItemsCount = ItemsToShow.Count();
            TotalPages = (int)Math.Ceiling(ItemsCount / (decimal)PageSize);
        }

        protected void NextPage()
        {
            CurrentPage++;
            StateHasChanged();
        }

        protected void PrevPage()
        {
            CurrentPage--;
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
