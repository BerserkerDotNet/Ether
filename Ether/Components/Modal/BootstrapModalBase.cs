using Microsoft.AspNetCore.Components;
using System;

namespace Ether.Components.Modal
{
    public class BootstrapModalBase : ComponentBase, IDisposable
    {
        [Inject] public ModalService Service { get; set; }

        public string Title { get; set; }

        public bool IsVisible { get; set; }

        public RenderFragment Content { get; set; }

        public void Dispose()
        {
            Service.OnShow -= ShowModal;
            Service.OnClose -= CloseModal;
        }

        protected override void OnInit()
        {
            Service.OnShow += ShowModal;
            Service.OnClose += CloseModal;

            StateHasChanged();
        }

        protected void ShowModal(string title, RenderFragment content, ModalAction[] actions)
        {
            Title = title;
            IsVisible = true;
            Content = content;

            StateHasChanged();
        }

        protected void CloseModal()
        {
            Title = string.Empty;
            IsVisible = false;
            Content = null;
        }
    }
}
