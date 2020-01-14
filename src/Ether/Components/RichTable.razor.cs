using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Types;
using Ether.Types.Interfaces;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components
{
    public class RichTableBase<TItem> : ComponentBase, IRichTable<TItem>
    {
        private static readonly Func<TItem> TCreator = Expression.Lambda<Func<TItem>>(Expression.New(typeof(TItem).GetConstructor(Type.EmptyTypes))).Compile();

        [Parameter]
        public string GridTitle { get; set; }

        [Parameter]
        public string FormTitle { get; set; }

        [Parameter]
        public IEnumerable<TItem> Items { get; set; }

        [Parameter]
        public bool ShowPaging { get; set; } = true;

        [Parameter]
        public bool ShowAdd { get; set; } = true;

        [Parameter]
        public int PageSize { get; set; } = 10;

        [Parameter]
        public RenderFragment TableHeader { get; set; }

        [Parameter]
        public RenderFragment<RichTableContext<TItem>> TableBody { get; set; }

        [Parameter]
        public RenderFragment<RichTableContext<TItem>> FormBody { get; set; }

        [Parameter]
        public EventCallback OnRefresh { get; set; }

        [Parameter]
        public EventCallback OnCancelEdit { get; set; }

        [Parameter]
        public EventCallback<TItem> OnSaveEdit { get; set; }

        protected bool IsEditing { get; set; }

        protected TItem EditingItem { get; set; } = TCreator();

        public RenderFragment TableBodyInternal(TItem item) => TableBody(CreateContext(item));

        public RenderFragment FormBodyInternal(TItem item) => FormBody != null ? FormBody(CreateContext(item)) : null;

        protected override void OnParametersSet()
        {
            if (Items == null)
            {
                Items = Enumerable.Empty<TItem>();
            }
        }

        public void Edit(TItem item)
        {
            EditInternal(item, clone: true);
        }

        protected void New()
        {
            var item = TCreator();
            if (item is ViewModelWithId vm)
            {
                vm.Id = Guid.NewGuid();
            }

            EditInternal(item, clone: false);
        }

        protected Task SaveEdit()
        {
            IsEditing = false;
            return OnSaveEdit.InvokeAsync(EditingItem);
        }

        protected RichTableContext<TItem> CreateContext(TItem item)
        {
            return new RichTableContext<TItem>(item, this);
        }

        protected Task CancelEdit()
        {
            IsEditing = false;
            return OnCancelEdit.InvokeAsync(null);
        }

        private void EditInternal(TItem item, bool clone)
        {
            IsEditing = true;
            EditingItem = clone ? item.PoorMansClone() : item;
        }
    }
}
