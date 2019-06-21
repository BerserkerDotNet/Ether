using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Types;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Ether.Components.Code
{
    public class EditableTableBase<T> : ComponentBase
    {
        private static readonly Func<T> TCreator = Expression.Lambda<Func<T>>(Expression.New(typeof(T).GetConstructor(Type.EmptyTypes))).Compile();

        private T[] _items = new T[0];

        private T _shadowCopy;

        public IEnumerable<T> Items
        {
            get { return _items; }
        }

        public T EditingItem { get; set; }

        [Parameter]
        protected Func<T, object> OrderBy { get; set; }

        [Parameter]
        protected Func<T, object> OrderByDescending { get; set; }

        [Inject]
        protected EtherClient Client { get; set; }

        [Inject]
        protected ILogger<EditableTableBase<T>> Logger { get; set; }

        [Inject]
        protected JsUtils JsUtils { get; set; }

        protected bool IsEditing { get; set; }

        protected bool IsLoading { get; set; }

        [Parameter]
        protected Action<EditableTableBase<T>> OnRecordsLoaded { get; set; }

        public virtual void Edit(T model)
        {
            StartEditing(model);
        }

        public virtual async Task Delete(T model)
        {
            var delete = await JsUtils.Confirm($"Are you sure you want to delete selected item?");
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
                var items = await Client.GetAll<T>();
                if (OrderByDescending != null)
                {
                    items = items.OrderByDescending(OrderByDescending);
                }

                if (OrderBy != null)
                {
                    items = items.OrderBy(OrderBy);
                }

                _items = items.ToArray();
                OnRecordsLoaded?.Invoke(this);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Refresh failed");
                await JsUtils.NotifyError("Error loading records", ex.Message);
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
            _shadowCopy = item.PoorMansClone();
            EditingItem = item;
            IsEditing = true;
            StateHasChanged();
        }

        protected void FinishEditing()
        {
            _shadowCopy = default;
            EditingItem = default;
            IsEditing = false;
            StateHasChanged();
        }

        protected virtual void New()
        {
            Edit(TCreator());
        }

        protected virtual async Task Save()
        {
            try
            {
                await Client.Save(EditingItem);
                await JsUtils.NotifySuccess("Success", $"{EditingItem.GetType().Name.Replace("ViewModel", string.Empty)} was saved successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving record");
                await JsUtils.NotifyError("Error saving record", ex.Message);
                return;
            }

            await Refresh();
            FinishEditing();
        }

        protected virtual void Cancel()
        {
            var idx = Array.IndexOf(_items, EditingItem);
            if (idx != -1)
            {
                _items[idx] = _shadowCopy;
            }

            FinishEditing();
        }
    }
}
