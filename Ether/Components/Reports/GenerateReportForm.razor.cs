using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Actions.Async;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using static BlazorState.Redux.Blazor.ComponentConnector;

namespace Ether.Components.Reports
{
    public class GenerateReportFormProps
    {
        public Guid Profile { get; set; }

        public string ReportType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Dictionary<Guid, string> Profiles { get; set; }

        public Dictionary<string, string> ReportTypes { get; set; }

        public EventCallback<GenerateReportViewModel> OnGenerate { get; set; }

    }

    public class GenerateReportFormConnected
    {
        public static RenderFragment Get()
        {
            var connected = new GenerateReportFormConnected();
            return Connect<GenerateReportForm, RootState, GenerateReportFormProps>(connected.MapStateToProps, connected.MapDispatchToProps, connected.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (!IsReportTypesStateFetched(store.State))
            {
                await store.Dispatch<FetchReportDescriptors>();
            }

            if (!IsProfilesStateFetched(store.State))
            {
                await store.Dispatch<FetchProfiles>();
            }
        }

        private void MapStateToProps(RootState state, GenerateReportFormProps props)
        {
            var formState = GetForm(state);
            if (formState != null)
            {
                props.Profile = formState.Profile;
                props.ReportType = formState.ReportType;
                props.StartDate = formState.Start;
                props.EndDate = formState.End;
            }

            props.ReportTypes = GetReportTypes(state).ToDictionary(k => k.UniqueName, v => v.DisplayName);
            props.Profiles = GetProfiles(state).ToDictionary(k => k.Id, v => v.Name);

            if (props.Profile == default && props.Profiles.Any())
            {
                props.Profile = props.Profiles.First().Key;
            }

            if (props.ReportType == default && props.ReportTypes.Any())
            {
                props.ReportType = props.ReportTypes.First().Key;
            }
        }

        private IEnumerable<ProfileViewModel> GetProfiles(RootState state)
        {
            return state?.Profiles?.Profiles ?? Enumerable.Empty<ProfileViewModel>();
        }

        private bool IsProfilesStateFetched(RootState state)
        {
            return state?.Profiles != null;
        }

        private bool IsReportTypesStateFetched(RootState state)
        {
            return state?.GenerateReportForm?.ReportTypes != null;
        }

        private IEnumerable<ReporterDescriptorViewModel> GetReportTypes(RootState state)
        {
            return state?.GenerateReportForm?.ReportTypes ?? Enumerable.Empty<ReporterDescriptorViewModel>();
        }

        private GenerateReportViewModel GetForm(RootState state)
        {
            return state?.GenerateReportForm?.Form;
        }

        private void MapDispatchToProps(IStore<RootState> store, GenerateReportFormProps props)
        {
            props.OnGenerate = EventCallback.Factory.Create<GenerateReportViewModel>(this, r => HandleGenerate(store, r));
        }

        private Task HandleGenerate(IDispatcher dispatcher, GenerateReportViewModel reportRequest)
        {
            return dispatcher.Dispatch<GenerateReportAction, GenerateReportViewModel>(reportRequest);
        }
    }
}
