using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Blazor;
using BlazorState.Redux.Interfaces;
using Ether.Actions.Async;
using Ether.Types;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Settings
{
    public class ProjectsProps
    {
        public IEnumerable<VstsProjectViewModel> Items { get; set; }

        public IEnumerable<SelectOption<Guid>> OrganizationsOptions { get; set; }

        public IEnumerable<SelectOption<Guid>> IdentitiesOptions { get; set; }

        public EventCallback<VstsProjectViewModel> OnSave { get; set; }

        public EventCallback<VstsProjectViewModel> OnDelete { get; set; }

        public EventCallback OnRefresh { get; set; }
    }

    public class ProjectsConnected
    {
        public static RenderFragment Get()
        {
            var c = new ProjectsConnected();
            return ComponentConnector.Connect<Projects, RootState, ProjectsProps>(c.MapStateToProps, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (!IsOrganizationsInitialized(store.State))
            {
                await store.Dispatch<FetchOrganizations>();
            }

            if (!IsIdentitiesInitialized(store.State))
            {
                await store.Dispatch<FetchIdentities>();
            }

            if (!IsProjectsInitialized(store.State))
            {
                await store.Dispatch<FetchProjects>();
            }
        }

        private void MapStateToProps(RootState state, ProjectsProps props)
        {
            props.Items = GetProjects(state);
            props.OrganizationsOptions = GetOrganizationOptions(state);
            props.IdentitiesOptions = GetIdentityOptions(state);
        }

        private void MapDispatchToProps(IStore<RootState> store, ProjectsProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnSave = EventCallback.Factory.Create<VstsProjectViewModel>(this, p => HandleSave(store, p));
            props.OnDelete = EventCallback.Factory.Create<VstsProjectViewModel>(this, p => HandleDelete(store, p));
        }

        private bool IsProjectsInitialized(RootState state)
        {
            return state?.Projects?.Projects != null;
        }

        private bool IsOrganizationsInitialized(RootState state)
        {
            return state?.Settings?.Organizations != null;
        }

        private bool IsIdentitiesInitialized(RootState state)
        {
            return state?.Settings?.Identities != null;
        }

        private IEnumerable<VstsProjectViewModel> GetProjects(RootState state)
        {
            return state?.Projects?.Projects ?? null;
        }

        private IEnumerable<SelectOption<Guid>> GetOrganizationOptions(RootState state)
        {
            var organizations = state?.Settings?.Organizations ?? Enumerable.Empty<OrganizationViewModel>();
            var organizationsOptions = new List<SelectOption<Guid>>(organizations.Count() + 1);

            organizationsOptions.Add(new SelectOption<Guid>(Guid.Empty, string.Empty));

            foreach (var organization in organizations)
            {
                organizationsOptions.Add(new SelectOption<Guid>(organization.Id, organization.Name));
            }

            return organizationsOptions;
        }

        private IEnumerable<SelectOption<Guid>> GetIdentityOptions(RootState state)
        {
            var identities = state?.Settings?.Identities ?? Enumerable.Empty<IdentityViewModel>();
            var identitiesOptions = new List<SelectOption<Guid>>(identities.Count() + 1);

            identitiesOptions.Add(new SelectOption<Guid>(Guid.Empty, string.Empty));

            foreach (var identity in identities)
            {
                identitiesOptions.Add(new SelectOption<Guid>(identity.Id, identity.Name));
            }

            return identitiesOptions;
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            await store.Dispatch<FetchProjects>();
        }

        private async Task HandleSave(IStore<RootState> store, VstsProjectViewModel project)
        {
            var organization = store?.State?.Settings?.Organizations.Where(o => o.Id == project.Organization).FirstOrDefault();
            var identity = store?.State?.Settings?.Identities.Where(i => i.Id == organization.Identity).FirstOrDefault();

            project.Identity = identity.Id;

            await store.Dispatch<SaveProject, VstsProjectViewModel>(project);
        }

        private async Task HandleDelete(IStore<RootState> store, VstsProjectViewModel project)
        {
            await store.Dispatch<DeleteProject, VstsProjectViewModel>(project);
        }
    }
}
