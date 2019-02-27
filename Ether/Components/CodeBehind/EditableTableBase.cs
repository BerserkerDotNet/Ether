using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Types;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using static Ether.Types.JsUtils;

namespace Ether.Components.CodeBehind
{
    public class EditableTableBase<T> : ComponentBase, IFormHandler, IDisposable
    {
        private static readonly Func<T> TCreator = Expression.Lambda<Func<T>>(Expression.New(typeof(T).GetConstructor(Type.EmptyTypes))).Compile();

        private IFormValidator _formValidator;

        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public T EditingItem { get; set; }

        [Inject]
        protected EtherClient Client { get; set; }

        protected bool IsEditing { get; set; }

        protected bool IsLoading { get; set; }

        [Parameter]
        protected Action<EditableTableBase<T>> OnRecordsLoaded { get; set; }

        public void SetValidator(IFormValidator validator)
        {
            _formValidator = validator;
        }

        public void Dispose()
        {
            _formValidator = null;
        }

        public virtual void Edit(T model)
        {
            StartEditing(model);
        }

        public virtual async Task Delete(T model)
        {
            var delete = await Confirm($"Are you sure you want to delete selected item?");
            if (delete)
            {
                var viewModelWithId = model as ViewModelWithId;
                if (viewModelWithId == null)
                {
                    throw new NotSupportedException($"View model '{typeof(T).Name}' should be inherited from {nameof(ViewModelWithId)}");
                }

                await Client.Delete<T>(viewModelWithId.Id);
                await Refresh();
            }
        }

        public virtual async Task Refresh()
        {
            try
            {
                IsLoading = true;
                Items = await Client.GetAll<T>();
                OnRecordsLoaded?.Invoke(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:", ex);
                await NotifyError("Error loading records", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }

            StateHasChanged();
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
            EditingItem = default(T);
            IsEditing = false;
            StateHasChanged();
        }

        protected virtual void New()
        {
            Edit(TCreator());
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

            try
            {
                await Client.Save(EditingItem);
                await NotifySuccess("Success", $"{EditingItem.GetType().Name.Replace("ViewModel", string.Empty)} was saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:", ex);
                await NotifyError("Error saving record", ex.Message);
                return;
            }

            await Refresh();
            FinishEditing();
        }

        protected virtual void Cancel()
        {
            FinishEditing();
        }
    }
}
