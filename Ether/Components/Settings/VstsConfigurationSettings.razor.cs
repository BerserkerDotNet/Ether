using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Actions.Async;
using Ether.Redux.Interfaces;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using static Ether.Redux.Blazor.ComponentConnector;

namespace Ether.Components.Settings
{

    public class VstsConfigurationSettingsProps
    {
        public VstsDataSourceViewModel VstsDataSource { get; set; }

        public Dictionary<Guid?, string> IdentitiesOptions { get; set; }

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
            if (store.State?.Settings?.DataSource == null)
            {
                await store.Dispatch<FetchDataSourceSettings>();
            }

            if (!IsIdentityOptionsInitialized(store.State))
            {
                await store.Dispatch<FetchIdentities>();
            }
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

        private Dictionary<Guid?, string> GetIdentityOptions(RootState state)
        {
            var identities = state?.Settings?.Identities ?? Enumerable.Empty<IdentityViewModel>();
            var options = new Dictionary<Guid?, string>(identities.Count() + 1);
            options.Add(Guid.Empty, Constants.NoneLabel);
            foreach (var identity in identities)
            {
                options.Add(identity.Id, identity.Name);
            }

            return options;
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
