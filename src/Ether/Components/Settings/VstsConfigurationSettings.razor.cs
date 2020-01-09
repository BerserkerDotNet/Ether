using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Actions.Async;
using Ether.Types;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using static BlazorState.Redux.Blazor.ComponentConnector;

namespace Ether.Components.Settings
{

    public class VstsConfigurationSettingsProps
    {
        public VstsDataSourceViewModel VstsDataSource { get; set; }

        public IEnumerable<SelectOption<Guid?>> IdentitiesOptions { get; set; }

        public EventCallback<VstsDataSourceViewModel> OnSave { get; set; }
    }

    public class VstsConfigurationSettingsConnected
    {
        public static RenderFragment Get()
        {
            var c = new VstsConfigurationSettingsConnected();
            return Connect<VstsConfigurationSettings, RootState, VstsConfigurationSettingsProps>(c.MapStateToProp, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            await store.Dispatch<FetchIdentities>();
            await store.Dispatch<FetchDataSourceSettings>();
        }

        private void MapStateToProp(RootState state, VstsConfigurationSettingsProps props)
        {
            props.VstsDataSource = GetVstsDataSource(state);
            props.IdentitiesOptions = GetIdentityOptions(state);
        }

        private void MapDispatchToProps(IStore<RootState> store, VstsConfigurationSettingsProps props)
        {
            props.OnSave = EventCallback.Factory.Create<VstsDataSourceViewModel>(this, s => HandleSave(store, s));
        }

        private async Task HandleSave(IStore<RootState> store, VstsDataSourceViewModel s)
        {
            await store.Dispatch<SaveDataSource, VstsDataSourceViewModel>(s);
        }

        private IEnumerable<SelectOption<Guid?>> GetIdentityOptions(RootState state)
        {
            var identities = state?.Settings?.Identities ?? Enumerable.Empty<IdentityViewModel>();
            var identitiesOptions = new List<SelectOption<Guid?>>(identities.Count() + 1);
            identitiesOptions.Add(new SelectOption<Guid?>(Guid.Empty, Constants.NoneLabel));
            foreach (var identity in identities)
            {
                identitiesOptions.Add(new SelectOption<Guid?>(identity.Id, identity.Name));
            }

            return identitiesOptions;
        }

        private bool IsIdentityOptionsInitialized(RootState state)
        {
            return state?.Settings?.Identities != null;
        }

        private VstsDataSourceViewModel GetVstsDataSource(RootState state)
        {
            return state?.Settings?.DataSource;
        }
    }
}
