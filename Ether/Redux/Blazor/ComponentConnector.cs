using System;
using Ether.Redux.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Ether.Redux.Blazor
{
    public static class ComponentConnector
    {
        public static RenderFragment Connect<TComponent, TState, TProps>(Action<TState, TProps> mapStateToProps, Action<IStore<TState>, TProps> mapDispatchToProps)
                    where TComponent : ComponentBase
                    where TProps : new()
        {
            return new RenderFragment(builder =>
            {
                builder.OpenComponent<ComponentConnected<TComponent, TState, TProps>>(1);
                builder.AddAttribute(2, "MapStateToProps", mapStateToProps);
                builder.AddAttribute(2, "MapDispatchToProps", mapDispatchToProps);
                builder.CloseComponent();
            });
        }
    }
}
