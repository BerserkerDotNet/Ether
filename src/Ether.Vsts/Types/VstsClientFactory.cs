using System;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Ether.Vsts.Exceptions;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using VSTS.Net;
using VSTS.Net.Interfaces;
using VSTS.Net.Types;

namespace Ether.Vsts.Types
{
    public class VstsClientFactory : IVstsClientFactory
    {
        private readonly IMediator _mediator;

        public VstsClientFactory(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IVstsClient Client { get; private set; }

        public async Task<IVstsClient> GetClient(Guid organizationId, string token = null)
        {
            OrganizationViewModel config;

            if (organizationId != default)
            {
                config = await _mediator.Request<Queries.GetOrganizationById, OrganizationViewModel>(new Queries.GetOrganizationById(organizationId));
            }
            else
            {
                config = await _mediator.Request<GetFirstOrganization, OrganizationViewModel>();
            }

            if (config == null || config.Identity.Equals(Guid.Empty))
            {
                throw new AzureDevopsConfigurationIsMissingException();
            }

            if (string.IsNullOrEmpty(token))
            {
                var identity = await _mediator.Request<GetIdentityById, IdentityViewModel>(new GetIdentityById { Id = config.Identity });
                token = identity.Token;
            }

            Client = VstsClient.Get(new OnlineUrlBuilderFactory(config.Name), token);

            return Client;
        }

        public async Task<IVstsIdentityClient> GetIdentityClient(string token = null)
        {
            var client = await GetClient(default, token);

            return client;
        }

        public async Task<IVstsPullRequestsClient> GetPullRequestsClient(Guid organizationId, string token = null)
        {
            var client = await GetClient(organizationId, token);

            return client;
        }

        public async Task<IVstsWorkItemsClient> GetWorkItemsClient(Guid? identityId)
        {
            var token = string.Empty;

            if (identityId.HasValue)
            {
                var identity = await _mediator.Request<GetIdentityById, IdentityViewModel>(new GetIdentityById { Id = identityId.Value });

                token = identity.Token;
            }

            var client = await GetClient(default, token);

            return client;
        }
    }
}