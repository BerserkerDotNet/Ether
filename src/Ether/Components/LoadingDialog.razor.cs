using BlazorState.Redux.Blazor;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;
using Microsoft.AspNetCore.Components;

namespace Ether.Components
{
    public class LoadingDialogProps
    {
        public bool IsLoading { get; set; }
    }

    public class LoadingDialogConnected
    {
        public static RenderFragment Get()
        {
            var c = new LoadingDialogConnected();
            return ComponentConnector.Connect<LoadingDialog, RootState, LoadingDialogProps>(c.MapStateToProps, c.MapDispatchToProps);
        }

        private void MapStateToProps(RootState state, LoadingDialogProps props)
        {
            props.IsLoading = state?.ComponentsInLoadingState > 0;
        }

        private void MapDispatchToProps(IStore<RootState> store, LoadingDialogProps props)
        {
        }
    }
}
