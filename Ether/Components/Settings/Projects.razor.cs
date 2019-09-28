using Ether.Actions.Async;
using BlazorState.Redux.Blazor;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Components.Settings
{
    public class ProjectsProps
    {
        public IEnumerable<VstsProjectViewModel> Items { get; set; }

        public Dictionary<Guid?, string> IdentitiesOptions { get; set; }

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

        private bool IsIdentitiesInitialized(RootState state)
        {
            return state?.Settings?.Identities != null;
        }

        private IEnumerable<VstsProjectViewModel> GetProjects(RootState state)
        {
            return state?.Projects?.Projects ?? null;
        }

        private Dictionary<Guid?, string> GetIdentityOptions(RootState state)
        {
            var identities = state?.Settings?.Identities ?? Enumerable.Empty<IdentityViewModel>();
            var identitiesOptions = new Dictionary<Guid?, string>(identities.Count() + 1);
            identitiesOptions.Add(Guid.Empty, Constants.NoneLabel);
            foreach (var identity in identities)
            {
                identitiesOptions.Add(identity.Id, identity.Name);
            }

            return identitiesOptions;
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            await store.Dispatch<FetchProjects>();
        }

        private async Task HandleSave(IStore<RootState> store, VstsProjectViewModel project)
        {
            await store.Dispatch<SaveProject, VstsProjectViewModel>(project);
        }

        private async Task HandleDelete(IStore<RootState> store, VstsProjectViewModel project)
        {
            await store.Dispatch<DeleteProject, VstsProjectViewModel>(project);
        }
    }
}
