using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Actions.Async;
using BlazorState.Redux.Blazor;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Settings
{
    public class ProfilesProps
    {
        public IEnumerable<ProfileViewModel> Items { get; set; }

        public Dictionary<Guid, string> MembersOptions { get; set; }

        public Dictionary<Guid, string> RepositoriesOptions { get; set; }

        public EventCallback<ProfileViewModel> OnChange { get; set; }

        public EventCallback<ProfileViewModel> OnDelete { get; set; }

        public EventCallback<IEnumerable<Guid>> OnFetchWorkItems { get; set; }

        public EventCallback<IEnumerable<Guid>> OnResetWorkItems { get; set; }

        public EventCallback<IEnumerable<Guid>> OnFetchPullRequests { get; set; }

        public EventCallback<IEnumerable<Guid>> OnResetPullRequests { get; set; }

        public EventCallback OnRefresh { get; set; }
    }

    public class ProfilesConnected
    {
        public static RenderFragment Get()
        {
            var c = new ProfilesConnected();
            return ComponentConnector.Connect<Profiles, RootState, ProfilesProps>(c.MapStateToProps, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (IsRepositoriesInitialized(store.State))
            {
                await store.Dispatch<FetchRepositories>();
            }

            if (IsMembersInitialized(store.State))
            {
                await store.Dispatch<FetchMembers>();
            }

            if (!IsProfilesInitialized(store.State))
            {
                await store.Dispatch<FetchProfiles>();
            }
        }

        private void MapStateToProps(RootState state, ProfilesProps props)
        {
            props.Items = GetProfiles(state);
            props.MembersOptions = GetMemberOptions(state);
            props.RepositoriesOptions = GetRepositoriesOptions(state);
        }

        private void MapDispatchToProps(IStore<RootState> store, ProfilesProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnChange = EventCallback.Factory.Create<ProfileViewModel>(this, p => HandleSave(store, p));
            props.OnDelete = EventCallback.Factory.Create<ProfileViewModel>(this, p => HandleDelete(store, p));
            props.OnFetchPullRequests = EventCallback.Factory.Create<IEnumerable<Guid>>(this, m => HandleFetchPullRequests(store, m));
            props.OnFetchWorkItems = EventCallback.Factory.Create<IEnumerable<Guid>>(this, m => HandleFetchWorkItems(store, m));
            props.OnResetPullRequests = EventCallback.Factory.Create<IEnumerable<Guid>>(this, m => HandleResetPullRequests(store, m));
            props.OnResetWorkItems = EventCallback.Factory.Create<IEnumerable<Guid>>(this, m => HandleResetWorkItems(store, m));
        }

        private IEnumerable<ProfileViewModel> GetProfiles(RootState state)
        {
            return state?.Profiles?.Profiles ?? null;
        }

        private Dictionary<Guid, string> GetMemberOptions(RootState state)
        {
            var members = state?.TeamMembers?.Members ?? Enumerable.Empty<TeamMemberViewModel>();
            return members.ToDictionary(k => k.Id, v => v.DisplayName);
        }

        private Dictionary<Guid, string> GetRepositoriesOptions(RootState state)
        {
            var repositories = state?.Repositories?.Repositories ?? Enumerable.Empty<VstsRepositoryViewModel>();
            return repositories.ToDictionary(k => k.Id, v => v.Name);
        }

        private bool IsProfilesInitialized(RootState state)
        {
            return state?.Profiles?.Profiles == null;
        }

        private bool IsMembersInitialized(RootState state)
        {
            return state?.TeamMembers?.Members == null;
        }

        private bool IsRepositoriesInitialized(RootState state)
        {
            return state?.Repositories?.Repositories == null;
        }

        private async Task HandleResetWorkItems(IStore<RootState> store, IEnumerable<Guid> members)
        {
            await store.Dispatch<FetchWorkItems, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = members,
                Reset = true
            });
        }

        private async Task HandleResetPullRequests(IStore<RootState> store, IEnumerable<Guid> members)
        {
            await store.Dispatch<FetchPullRequests, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = members,
                Reset = true
            });
        }

        private async Task HandleFetchWorkItems(IStore<RootState> store, IEnumerable<Guid> members)
        {
            await store.Dispatch<FetchWorkItems, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = members,
                Reset = false
            });
        }

        private async Task HandleFetchPullRequests(IStore<RootState> store, IEnumerable<Guid> members)
        {
            await store.Dispatch<FetchPullRequests, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = members,
                Reset = false
            });
        }

        private async Task HandleDelete(IStore<RootState> store, ProfileViewModel profile)
        {
            await store.Dispatch<DeleteProfile, ProfileViewModel>(profile);
        }

        private async Task HandleSave(IStore<RootState> store, ProfileViewModel profile)
        {
            await store.Dispatch<SaveProfile, ProfileViewModel>(profile);
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            await store.Dispatch<FetchProfiles>();
        }
    }
}
