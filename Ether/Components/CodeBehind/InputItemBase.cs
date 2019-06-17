using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Ether.Components.CodeBehind
{
    public class InputItemBase<T> : ComponentBase
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
                Console.WriteLine("Get Value");
                return _internalValue;
            }

            set
            {
                Console.WriteLine("Set Value: ", value);
                _internalValue = value;
                if (ValueChanged.HasDelegate)
                {
                    // ValueChanged.InvokeAsync(_internalValue);
                }
            }
        }

        [Parameter]
        protected EventCallback<T> ValueChanged { get; set; }

        protected string[] Properties => new[] { PropertyName };
    }
}
