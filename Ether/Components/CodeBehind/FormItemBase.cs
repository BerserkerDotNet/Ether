using System;
using System.Linq;
using Ether.Types;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;

namespace Ether.Components.CodeBehind
{
    public class FormItemBase : BlazorComponent, IDisposable
    {
        protected string Id { get; set; } = Guid.NewGuid().ToString();

        protected string ErrorMessage { get; set; } = string.Empty;

        [CascadingParameter]
        protected IFormValidator ContainerForm { get; set; }

        [Parameter]
        protected string Title { get; set; }

        [Parameter]
        protected string PropertyName { get; set; }

        [Parameter]
        protected string[] Properties { get; set; }

        [Parameter]
        protected RenderFragment ChildContent { get; set; }

        public void Dispose()
        {
            ContainerForm.OnValidated -= OnValidated;
        }

        protected override void OnInit()
        {
            if (Properties == null)
            {
                Properties = new[] { PropertyName };
            }

            ContainerForm.OnValidated += OnValidated;
        }

        private async void OnValidated(object sender, ValidationResult result)
        {
            if (result.IsValid || !result.Errors.Keys.Any(k => Properties.Contains(k)))
            {
                await JsUtils.SucceedValidation(Id);
                return;
            }

            var allErrors = result.Errors
                .Where(e => Properties.Contains(e.Key))
                .SelectMany(e => e.Value);
            ErrorMessage = string.Join(Environment.NewLine, allErrors);
            await JsUtils.FailValidation(Id);
            StateHasChanged();
        }
    }
}
