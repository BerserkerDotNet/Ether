using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Types;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Ether.Components
{
    public class EditableGridBase<TItem> : ComponentBase
    {
        private static readonly Func<TItem> TCreator = Expression.Lambda<Func<TItem>>(Expression.New(typeof(TItem).GetConstructor(Type.EmptyTypes))).Compile();

        [Inject]
        protected JsUtils JsUtils { get; set; }

        [Parameter]
        public IEnumerable<TItem> Items { get; set; }

        [Parameter]
        public string GridTitle { get; set; }

        [Parameter]
        public string NewTitle { get; set; }

        [Parameter]
        public string ExistingTitle { get; set; }

        [Parameter]
        public string CancelButtonText { get; set; } = "Cancel";

        [Parameter]
        public bool IsReadOnly { get; set; }

        [Parameter]
        public Func<TItem, object> OrderBy { get; set; }

        [Parameter]
        public Func<TItem, object> OrderByDescending { get; set; }

        [Parameter]
        public EventCallback<TItem> OnSave { get; set; }

        [Parameter]
        public EventCallback<TItem> OnDelete { get; set; }

        [Parameter]
        public EventCallback OnRefresh { get; set; }

        [Parameter]
        public RenderFragment<EditableGridBase<TItem>> ListModeContent { get; set; }

        [Parameter]
        public RenderFragment<EditableGridBase<TItem>> FormModeContent { get; set; }

        public TItem EditingItem { get; set; }

        protected bool IsEditing => EditingItem != null;

        protected bool IsLoading => Items == null;

        protected bool IsSaving { get; set; }

        public void Edit(TItem item)
        {
            Editing(item);
        }

        public async Task Delete(TItem item)
        {
            if (!OnDelete.HasDelegate)
            {
                return;
            }

            var delete = await JsUtils.Confirm($"Are you sure you want to delete selected item?");
            if (delete)
            {
                var viewModelWithId = item as ViewModelWithId;
                if (viewModelWithId == null)
                {
                    throw new NotSupportedException($"View model '{typeof(TItem).Name}' should be inherited from {nameof(ViewModelWithId)}");
                }

                await OnDelete.InvokeAsync(item);
            }
        }

        protected async Task Save()
        {
            if (OnSave.HasDelegate)
            {
                IsSaving = true;
                await OnSave.InvokeAsync(EditingItem);
            }

            Editing(default);
        }

        protected void Cancel()
        {
            Editing(default);
        }

        protected void OnNew(MouseEventArgs args)
        {
            var item = TCreator();
            if (item is ViewModelWithId vm)
            {
                vm.Id = Guid.NewGuid();
            }

            Edit(item);
        }

        private void Editing(TItem item)
        {
            EditingItem = item;
            IsSaving = false;
        }
    }
}
