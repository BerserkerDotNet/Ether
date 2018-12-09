using System;
using Microsoft.AspNetCore.Blazor.Components;

namespace Ether.Components.CodeBehind
{
    public class InputItemBase<T> : BlazorComponent
    {
        private T _internalValue = default(T);

        [Parameter]
        protected string Title { get; set; }

        [Parameter]
        protected bool NoLabel { get; set; }

        [Parameter]
        protected string PropertyName { get; set; }

        [Parameter]
        protected T Value
        {
            get
            {
                return _internalValue;
            }

            set
            {
                _internalValue = value;
                ValueChanged?.Invoke(_internalValue);
                Console.WriteLine($"Value of {this.GetType().Name} is set to {value}");
            }
        }

        [Parameter]
        protected Action<T> ValueChanged { get; set; }

        protected string[] Properties => new[] { PropertyName };
    }
}
