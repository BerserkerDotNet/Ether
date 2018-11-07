using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Types;
using Ether.ViewModels;
using Microsoft.AspNetCore.Blazor.Components;
using static Ether.Types.JsUtils;

namespace Ether.Components.CodeBehind
{
    public class EditableTable<T> : BlazorComponent
        where T : ViewModelWithId, new()
    {
        [Inject]
        protected EtherClient Client { get; set; }

        protected IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        protected T EditingItem { get; set; }

        protected bool IsEditing { get; set; }

        protected override async Task OnInitAsync()
        {
            await Refresh();
        }

        protected void StartEditing(T item)
        {
            EditingItem = item;
            IsEditing = true;
            StateHasChanged();
        }

        protected void FinishEditing()
        {
            EditingItem = null;
            IsEditing = false;
            StateHasChanged();
        }

        protected virtual void New()
        {
            Edit(new T());
        }

        protected virtual void Edit(T model)
        {
            StartEditing(model);
        }

        protected virtual async Task Save()
        {
            await Client.Save(EditingItem);
            await Refresh();
            FinishEditing();
        }

        protected virtual void Cancel()
        {
            FinishEditing();
        }

        protected virtual async Task Delete(T model)
        {
            var delete = await Confirm($"Are you sure you want to delete selected item?");
            if (delete)
            {
                await Client.Delete<T>(model.Id);
                await Refresh();
            }
        }

        private async Task Refresh()
        {
            Items = await Client.GetAll<T>();
            StateHasChanged();
        }
    }
}
