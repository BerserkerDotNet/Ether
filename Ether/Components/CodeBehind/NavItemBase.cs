using System;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.CodeBehind
{
    public abstract class NavItemBase : ComponentBase, IDisposable
    {
        private bool _isActive = false;
        private string[] _absoluteUrls;

        [Inject]
        public IUriHelper UriHelper { get; set; }

        public void Dispose()
        {
            UriHelper.OnLocationChanged -= OnLocationChanged;
        }

        protected bool IsActive => _isActive;

        protected override void OnInit()
        {
            base.OnInit();
            UriHelper.OnLocationChanged += OnLocationChanged;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            _absoluteUrls = GetRelativeUrlPrefixes()
                .Select(u => UriHelper.ToAbsoluteUri(u).AbsoluteUri)
                .ToArray();
            OnLocationChanged(this, UriHelper.GetAbsoluteUri());
        }

        protected abstract string[] GetRelativeUrlPrefixes();

        private void OnLocationChanged(object sender, string newUriAbsolute)
        {
            var shouldBeActiveNow = _absoluteUrls.Any(u => EqualsHrefExactlyOrIfTrailingSlashAdded(newUriAbsolute, u));
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
