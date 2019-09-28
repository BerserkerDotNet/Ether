using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Ether.Components.Code
{
    public class SelectInput<T> : InputSelect<T>
    {
        [Parameter] public Dictionary<T, string> Options { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "select");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "class", CssClass);
            builder.AddAttribute(3, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(4, "onchange", EventCallback.Factory.CreateBinder<string>(this, val => CurrentValueAsString = val, CurrentValueAsString));
            builder.AddContent(5, RenderOptions);
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
                var seq = 0;
                builder.OpenElement(seq++, "option");
                builder.SetKey(option.Key);
                builder.AddAttribute(seq++, "value", option.Key);
                if (Equals(option.Key, CurrentValue))
                {
                    builder.AddAttribute(seq++, "selected", true);
                }

                builder.AddContent(seq++, option.Value);
                builder.CloseElement();
            }
        }
    }
}
