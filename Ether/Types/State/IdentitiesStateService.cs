using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class IdentitiesStateService
    {
        private readonly AppState _state;
        private readonly EtherClient _client;

        public IdentitiesStateService(AppState state, EtherClient client)
        {
            _state = state;
            _client = client;
        }

        public IEnumerable<IdentityViewModel> Identities => _state.Identities;

        public Dictionary<Guid?, string> IdentitiesOptions
        {
            get
            {
                var identities = Identities ?? Enumerable.Empty<IdentityViewModel>();
                var identitiesOptions = new Dictionary<Guid?, string>(identities.Count() + 1);
                identitiesOptions.Add(Guid.Empty, Constants.NoneLabel);
                foreach (var identity in identities)
                {
                    identitiesOptions.Add(identity.Id, identity.Name);
                }

                return identitiesOptions;
            }
        }

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.Identities = null;
            }

            if (_state.Identities == null)
            {
                _state.Identities = await _client.GetAll<IdentityViewModel>();
            }
        }

        public async Task UpdateAsync(IdentityViewModel identity)
        {
            await _client.Save(identity);
            if (!Identities.Any(i => i.Id == identity.Id))
            {
                await LoadAsync(hard: true);
            }
        }

        public async Task DeleteAsync(IdentityViewModel identity)
        {
            await _client.Delete<IdentityViewModel>(identity.Id);
            await LoadAsync(hard: true);
        }
    }
}
