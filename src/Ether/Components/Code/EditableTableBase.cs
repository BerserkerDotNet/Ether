using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Types;
using Ether.Types.EditableTable;
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

        private IEditableTableDataProvider _dataProvider;

        public IEnumerable<T> Items
        {
            get { return _items; }
        }

        public T EditingItem { get; set; }

        [Parameter]
        public Func<T, object> OrderBy { get; set; }

        [Parameter]
        public Func<T, object> OrderByDescending { get; set; }

        [Parameter]
        public Action<EditableTableBase<T>> OnRecordsLoaded { get; set; }

        [Parameter]
        public IEditableTableDataProvider DataProvider { get; set; }

        [Inject]
        public ILogger<EditableTableBase<T>> Logger { get; set; }

        [Inject]
        public EtherClientEditableTableDataProvider DefaultDataProvider { get; set; }

        [Inject]
        public JsUtils JsUtils { get; set; }

        protected bool IsEditing { get; set; }

        protected bool IsLoading { get; set; }

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

                await _dataProvider.Delete<T>(viewModelWithId.Id);
                await Refresh();
            }
        }

        public virtual async Task Refresh()
        {
            try
            {
                IsLoading = true;
                if (_dataProvider == null)
                {
                    return;
                }

                var items = await _dataProvider.Load<T>();
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
            }
            finally
            {
                IsLoading = false;
            }

            // StateHasChanged();
        }

        protected override async Task OnParametersSetAsync()
        {
            _dataProvider = DataProvider ?? DefaultDataProvider;

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
            var item = TCreator();
            if (item is ViewModelWithId vm)
            {
                vm.Id = Guid.NewGuid();
            }

            Edit(item);
        }

        protected virtual async Task Save()
        {
            try
            {
                await _dataProvider.Save(EditingItem);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving record");
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
