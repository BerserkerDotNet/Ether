using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Ether.Components.Code
{
    public class SelectInput<T> : InputSelect<T>
    {
        [Parameter] public Dictionary<T, string> Options { get; private set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "select");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "id", Id);
            builder.AddAttribute(3, "class", CssClass);
            builder.AddAttribute(4, "value", BindMethods.GetValue(CurrentValueAsString));
            builder.AddAttribute(5, "onchange", BindMethods.SetValueHandler(val => CurrentValueAsString = val, CurrentValueAsString));
            builder.AddContent(6, RenderOptions);
            builder.CloseElement();
        }

        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
            {
                var isParsed = Guid.TryParse(value, out var guid);
                if (isParsed)
                {
                    result = (T)(object)guid;
                    validationErrorMessage = null;
                    return true;
                }
            }

            return base.TryParseValueFromString(value, out result, out validationErrorMessage);
        }

        private void RenderOptions(RenderTreeBuilder builder)
        {
            foreach (var option in Options)
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", option.Key);
                builder.AddContent(2, option.Value);
                builder.CloseElement();
            }
        }
    }
}
