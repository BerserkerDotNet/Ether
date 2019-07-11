using System;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Modal
{
    public class ModalService
    {
        public event Action<string, RenderFragment, ModalAction[]> OnShow;

        public event Action OnClose;

        public void Show<T>(string title, ModalAction[] actions, params ModalParameter[] parameters)
            where T : ComponentBase
        {
            var content = new RenderFragment(x =>
            {
                x.OpenComponent(1, typeof(T));
                if (parameters != null && parameters.Any())
                {
                    var s = 2;
                    foreach (var parameter in parameters)
                    {
                        x.AddAttribute(s++, parameter.Name, parameter.Value);
                    }
                }

                x.CloseComponent();
            });

            OnShow?.Invoke(title, content, actions);
        }

        public void Close()
        {
            OnClose?.Invoke();
        }
    }
}
