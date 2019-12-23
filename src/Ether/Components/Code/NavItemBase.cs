using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Ether.Components.Code
{
    public abstract class NavItemBase : ComponentBase, IDisposable
    {
        private bool _isActive = false;
        private string[] _absoluteUrls;

        [Inject]
        public NavigationManager Navigation { get; set; }

        protected bool IsActive => _isActive;

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Navigation.LocationChanged += OnLocationChanged;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            _absoluteUrls = GetRelativeUrlPrefixes()
                .Select(u => Navigation.ToAbsoluteUri(u).AbsoluteUri)
                .ToArray();
            OnLocationChanged(this, new LocationChangedEventArgs(Navigation.Uri, false));
        }

        protected abstract string[] GetRelativeUrlPrefixes();

        private void OnLocationChanged(object sender, LocationChangedEventArgs args)
        {
            var shouldBeActiveNow = _absoluteUrls.Any(u => EqualsHrefExactlyOrIfTrailingSlashAdded(args.Location, u));
            if (shouldBeActiveNow != _isActive)
            {
                _isActive = shouldBeActiveNow;
                StateHasChanged();
            }
        }

        private bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute, string urlToCompare)
        {
            if (string.Equals(currentUriAbsolute, urlToCompare, StringComparison.Ordinal))
            {
                return true;
            }

            if (currentUriAbsolute.Length == urlToCompare.Length - 1)
            {
                if (urlToCompare[urlToCompare.Length - 1] == '/'
                    && urlToCompare.StartsWith(currentUriAbsolute, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
