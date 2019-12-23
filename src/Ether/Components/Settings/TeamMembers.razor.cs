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
    public class TeamMembersProps
    {
        public IEnumerable<TeamMemberViewModel> Members { get; set; }

        public IEnumerable<ProfileViewModel> Profiles { get; set; }

        public EventCallback<TeamMemberViewModel> OnChange { get; set; }

        public EventCallback<TeamMemberViewModel> OnDelete { get; set; }

        public EventCallback<TeamMemberViewModel> OnFetchWorkItems { get; set; }

        public EventCallback<TeamMemberViewModel> OnResetWorkItems { get; set; }

        public EventCallback<TeamMemberViewModel> OnFetchPullRequests { get; set; }

        public EventCallback<TeamMemberViewModel> OnResetPullRequests { get; set; }

        public EventCallback OnRefresh { get; set; }
    }

    public class TeamMembersConnected
    {
        public static RenderFragment Get()
        {
            var c = new TeamMembersConnected();
            return ComponentConnector.Connect<TeamMembers, RootState, TeamMembersProps>(c.MapStateToProps, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (!IsProfilesInitialized(store.State))
            {
                await store.Dispatch<FetchProfiles>();
            }

            if (!IsMembersInitialized(store.State))
            {
                await store.Dispatch<FetchMembers>();
            }
        }

        private void MapStateToProps(RootState state, TeamMembersProps props)
        {
            props.Members = GetMembers(state);
            props.Profiles = GetProfiles(state);
        }

        private void MapDispatchToProps(IStore<RootState> store, TeamMembersProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnChange = EventCallback.Factory.Create<TeamMemberViewModel>(this, m => HandleSave(store, m));
            props.OnDelete = EventCallback.Factory.Create<TeamMemberViewModel>(this, m => HandleDelete(store, m));
            props.OnFetchPullRequests = EventCallback.Factory.Create<TeamMemberViewModel>(this, m => HandleFetchPullRequests(store, m));
            props.OnFetchWorkItems = EventCallback.Factory.Create<TeamMemberViewModel>(this, m => HandleFetchWorkItems(store, m));
            props.OnResetPullRequests = EventCallback.Factory.Create<TeamMemberViewModel>(this, m => HandleResetPullRequests(store, m));
            props.OnResetWorkItems = EventCallback.Factory.Create<TeamMemberViewModel>(this, m => HandleResetWorkItems(store, m));
        }

        private IEnumerable<TeamMemberViewModel> GetMembers(RootState state)
        {
            return state?.TeamMembers?.Members ?? null;
        }

        private IEnumerable<ProfileViewModel> GetProfiles(RootState state)
        {
            return state?.Profiles?.Profiles ?? Enumerable.Empty<ProfileViewModel>();
        }

        private bool IsMembersInitialized(RootState state)
        {
            return state?.TeamMembers?.Members != null;
        }

        private bool IsProfilesInitialized(RootState state)
        {
            return state?.Profiles?.Profiles != null;
        }

        private async Task HandleResetWorkItems(IStore<RootState> store, TeamMemberViewModel member)
        {
            await store.Dispatch<FetchWorkItems, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = new[] { member.Id },
                Reset = true
            });
        }

        private async Task HandleResetPullRequests(IStore<RootState> store, TeamMemberViewModel member)
        {
            await store.Dispatch<FetchPullRequests, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = new[] { member.Id },
                Reset = true
            });
        }

        private async Task HandleFetchWorkItems(IStore<RootState> store, TeamMemberViewModel member)
        {
            await store.Dispatch<FetchWorkItems, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = new[] { member.Id },
                Reset = false
            });
        }

        private async Task HandleFetchPullRequests(IStore<RootState> store, TeamMemberViewModel member)
        {
            await store.Dispatch<FetchPullRequests, FetchDataJobParameters>(new FetchDataJobParameters
            {
                Members = new[] { member.Id },
                Reset = false
            });
        }

        private async Task HandleDelete(IStore<RootState> store, TeamMemberViewModel member)
        {
            await store.Dispatch<DeleteTeamMember, TeamMemberViewModel>(member);
        }

        private async Task HandleSave(IStore<RootState> store, TeamMemberViewModel member)
        {
            await store.Dispatch<SaveTeamMember, TeamMemberViewModel>(member);
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            await store.Dispatch<FetchProfiles>();
        }
    }
}
