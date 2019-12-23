using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Blazor;
using BlazorState.Redux.Interfaces;
using Ether.Actions.Async;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Settings
{
    public class RepositoriesProps
    {
        public IEnumerable<VstsRepositoryViewModel> Items { get; set; }

        public Dictionary<Guid, string> ProjectsOptions { get; set; }

        public EventCallback<VstsRepositoryViewModel> OnSave { get; set; }

        public EventCallback<VstsRepositoryViewModel> OnDelete { get; set; }

        public EventCallback OnRefresh { get; set; }
    }

    public class RepositoriesConnected
    {
        public static RenderFragment Get()
        {
            var c = new RepositoriesConnected();
            return ComponentConnector.Connect<Repositories, RootState, RepositoriesProps>(c.MapStateToProps, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (!IsProjectsInitialized(store.State))
            {
                await store.Dispatch<FetchProjects>();
            }

            if (!IsRepositoriesInitialized(store.State))
            {
                await store.Dispatch<FetchRepositories>();
            }
        }

        private void MapStateToProps(RootState state, RepositoriesProps props)
        {
            props.Items = GetRepositories(state);
            props.ProjectsOptions = GetProjectOptions(state);
        }

        private Dictionary<Guid, string> GetProjectOptions(RootState state)
        {
            var projects = state?.Projects?.Projects ?? Enumerable.Empty<VstsProjectViewModel>();
            var projectsOptions = new Dictionary<Guid, string>(projects.Count() + 1);
            projectsOptions.Add(Guid.Empty, Constants.NoneLabel);
            foreach (var project in projects)
            {
                projectsOptions.Add(project.Id, project.Name);
            }

            return projectsOptions;
        }

        private IEnumerable<VstsRepositoryViewModel> GetRepositories(RootState state)
        {
            return state?.Repositories?.Repositories ?? null;
        }

        private bool IsProjectsInitialized(RootState state)
        {
            return state?.Projects?.Projects != null;
        }

        private bool IsRepositoriesInitialized(RootState state)
        {
            return state?.Repositories?.Repositories != null;
        }

        private void MapDispatchToProps(IStore<RootState> store, RepositoriesProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnSave = EventCallback.Factory.Create<VstsRepositoryViewModel>(this, r => HandleSave(store, r));
            props.OnDelete = EventCallback.Factory.Create<VstsRepositoryViewModel>(this, r => HandleDelete(store, r));
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            await store.Dispatch<FetchRepositories>();
        }

        private async Task HandleDelete(IStore<RootState> store, VstsRepositoryViewModel repository)
        {
            await store.Dispatch<DeleteRepository, VstsRepositoryViewModel>(repository);
        }

        private async Task HandleSave(IStore<RootState> store, VstsRepositoryViewModel repository)
        {
            await store.Dispatch<SaveRepository, VstsRepositoryViewModel>(repository);
        }
    }
}
