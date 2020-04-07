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

namespace Ether.Components.Reports
{
    public class GenerateReportFormProps
    {
        public Guid Profile { get; set; }

        public string ReportType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public IEnumerable<SelectOption<Guid>> Profiles { get; set; }

        public IEnumerable<SelectOption<string>> ReportTypes { get; set; }

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
            await store.Dispatch<FetchProfiles>();
            await store.Dispatch<FetchReportDescriptors>();
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

            props.ReportTypes = GetReportTypes(state).Select(k => new SelectOption<string>(k.UniqueName, k.DisplayName)).ToArray();
            props.Profiles = GetProfiles(state).Select(k => new SelectOption<Guid>(k.Id, k.Name)).ToArray();

            if (props.Profile == default && props.Profiles.Any())
            {
                props.Profile = props.Profiles.First().Value;
            }

            if (props.ReportType == default && props.ReportTypes.Any())
            {
                props.ReportType = props.ReportTypes.First().Value;
            }
        }

        private IEnumerable<ProfileViewModel> GetProfiles(RootState state)
        {
            var profiles = state?.Profiles?.Profiles;
            if (profiles is null || !profiles.Any())
            {
                profiles = new[] { new ProfileViewModel { Id = Guid.Empty, Name = "None" } };
            }

            return profiles;
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
