using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Actions.Async;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using static BlazorState.Redux.Blazor.ComponentConnector;

namespace Ether.Components.Settings
{
    public class IdentitiesProps
    {
        public IEnumerable<IdentityViewModel> Items { get; set; }

        public EventCallback<IdentityViewModel> OnSave { get; set; }

        public EventCallback<IdentityViewModel> OnDelete { get; set; }

        public EventCallback OnRefresh { get; set; }
    }

    public class IdentitiesConnected
    {
        public static RenderFragment Get()
        {
            var c = new IdentitiesConnected();
            return Connect<Identities, RootState, IdentitiesProps>(c.MapStateToProps, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (store.State?.Settings?.Identities == null)
            {
                await store.Dispatch<FetchIdentities>();
            }
        }

        private void MapStateToProps(RootState state, IdentitiesProps props)
        {
            props.Items = GetIdentities(state);
        }

        private void MapDispatchToProps(IStore<RootState> store, IdentitiesProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnSave = EventCallback.Factory.Create<IdentityViewModel>(this, i => HandleSave(store, i));
            props.OnDelete = EventCallback.Factory.Create<IdentityViewModel>(this, i => HandleDelete(store, i));
        }

        private async Task HandleRefresh(IDispatcher dispatcher)
        {
            await dispatcher.Dispatch<FetchIdentities>();
        }

        private async Task HandleSave(IDispatcher dispatcher, IdentityViewModel identity)
        {
            await dispatcher.Dispatch<SaveIdentity, IdentityViewModel>(identity);
        }

        private async Task HandleDelete(IDispatcher dispatcher, IdentityViewModel identity)
        {
            await dispatcher.Dispatch<DeleteIdentity, IdentityViewModel>(identity);
        }

        private IEnumerable<IdentityViewModel> GetIdentities(RootState state)
        {
            return state?.Settings?.Identities ?? Enumerable.Empty<IdentityViewModel>();
        }
    }
}
