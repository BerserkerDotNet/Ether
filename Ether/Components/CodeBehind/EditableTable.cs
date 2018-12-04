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
    public class EditableTable<T> : BlazorComponent, IFormHandler, IDisposable
        where T : ViewModelWithId, new()
    {
        private IFormValidator _formValidator;

        [Inject]
        protected EtherClient Client { get; set; }

        protected IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        protected T EditingItem { get; set; }

        protected bool IsEditing { get; set; }

        public void SetValidator(IFormValidator validator)
        {
            _formValidator = validator;
        }

        public void Dispose()
        {
            _formValidator = null;
        }

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
            var model = EditingItem as IdentityViewModel;
            if (model != null)
            {
                Console.WriteLine($"Model: {model.Name}; {model.Token}");
            }

            if (_formValidator != null && !_formValidator.Validate(EditingItem))
            {
                return;
            }

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
