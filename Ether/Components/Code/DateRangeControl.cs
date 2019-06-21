using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;

namespace Ether.Components.Code
{
    public class DateRangeControl : ComponentBase
    {
        private ElementRef _dateRange;
        private FieldIdentifier _startIdentifier;
        private FieldIdentifier _endIdentifier;
        private bool _isInitialized = false;

        [Inject]
        public JsUtils Js { get; set; }

        [CascadingParameter] public EditContext EditContext { get; private set; }

        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; private set; }

        [Parameter] public string Id { get; private set; }

        [Parameter] public string Class { get; private set; }

        [Parameter] public DateTime Start { get; private set; }

        [Parameter] public EventCallback<DateTime> StartChanged { get; private set; }

        [Parameter] public Expression<Func<DateTime>> StartExpression { get; private set; }

        [Parameter] public DateTime End { get; private set; }

        [Parameter] public EventCallback<DateTime> EndChanged { get; private set; }

        [Parameter] public Expression<Func<DateTime>> EndExpression { get; private set; }

        protected string FieldClass
        {
            get
            {
                var startCls = EditContext.FieldClass(_startIdentifier);
                var endCls = EditContext.FieldClass(_endIdentifier);
                return startCls.Contains("invalid") ? startCls : endCls;
            }
        }

        protected string CssClass
            => string.IsNullOrEmpty(Class)
            ? FieldClass
            : $"{Class} {FieldClass}";

        [JSInvokable]
        public async Task OnRangeChanged(string start, string end)
        {
            Start = DateTime.Parse(start);
            await StartChanged.InvokeAsync(Start);
            var endDate = DateTime.Parse(end);
            End = endDate.AddDays(1).AddMilliseconds(-1);
            await EndChanged.InvokeAsync(End);

            EditContext.NotifyFieldChanged(_startIdentifier);
            EditContext.NotifyFieldChanged(_endIdentifier);

            this.StateHasChanged();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            _startIdentifier = FieldIdentifier.Create(StartExpression);
            _endIdentifier = FieldIdentifier.Create(EndExpression);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "id", Id);
            builder.AddAttribute(3, "class", CssClass);
            builder.AddElementReferenceCapture(4, r => _dateRange = r);
            builder.CloseElement();
        }

        protected override async Task OnAfterRenderAsync()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                await Js.DateRangePicker(_dateRange, DotNetObjectRef.Create<object>(this));
            }
        }
    }
}
