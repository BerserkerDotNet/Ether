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

    public class OrganizationsProps
    {
        public IEnumerable<OrganizationViewModel> Organizations { get; set; }

        public IEnumerable<string> TypesOptions { get; set; }

        public IEnumerable<SelectOption<Guid?>> IdentitiesOptions { get; set; }

        public EventCallback<OrganizationViewModel> OnSave { get; set; }

        public EventCallback<OrganizationViewModel> OnDelete { get; set; }

        public EventCallback OnRefresh { get; set; }
    }

    public class OrganizationsConnected
    {
        public static RenderFragment Get()
        {
            var c = new OrganizationsConnected();
            return Connect<Organizations, RootState, OrganizationsProps>(c.MapStateToProp, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            await store.Dispatch<FetchIdentities>();
            await store.Dispatch<FetchOrganizations>();
        }

        private void MapStateToProp(RootState state, OrganizationsProps props)
        {
            props.Organizations = GetOrganizations(state);
            props.TypesOptions = GetTypesOptions();
            props.IdentitiesOptions = GetIdentityOptions(state);
        }

        private void MapDispatchToProps(IStore<RootState> store, OrganizationsProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnSave = EventCallback.Factory.Create<OrganizationViewModel>(this, s => HandleSave(store, s));
            props.OnDelete = EventCallback.Factory.Create<OrganizationViewModel>(this, i => HandleDelete(store, i));
        }

        private async Task HandleRefresh(IDispatcher dispatcher)
        {
            await dispatcher.Dispatch<FetchOrganizations>();
        }

        private async Task HandleSave(IStore<RootState> store, OrganizationViewModel organization)
        {
            await store.Dispatch<SaveOrganization, OrganizationViewModel>(organization);
        }

        private async Task HandleDelete(IDispatcher dispatcher, OrganizationViewModel organization)
        {
            await dispatcher.Dispatch<DeleteOrganization, OrganizationViewModel>(organization);
        }

        // TODO: Implement dynamic collection filling
        private IEnumerable<string> GetTypesOptions()
        {
            var typesOptions = new List<string>() { string.Empty, "Vsts" };

            return typesOptions;
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

        private IEnumerable<OrganizationViewModel> GetOrganizations(RootState state)
        {
            return state?.Settings?.Organizations ?? null;
        }
    }
}
