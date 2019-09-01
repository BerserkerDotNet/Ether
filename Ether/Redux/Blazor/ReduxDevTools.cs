﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Ether.Redux.Blazor
{
    public class ReduxDevTools : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenElement(1, "script");
            builder.AddAttribute(2, "src", "/js/reduxdevtools.js");
            builder.CloseElement();
        }
    }
}