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
                return _internalValue;
            }

            set
            {
                _internalValue = value;
                ValueChanged?.Invoke(_internalValue);
            }
        }

        [Parameter]
        protected Action<T> ValueChanged { get; set; }

        protected string[] Properties => new[] { PropertyName };
    }
}
