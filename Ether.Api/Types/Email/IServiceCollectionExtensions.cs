using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Ether.EmailGenerator.EmailGenerator;

namespace Ether.Api.Types.Email
{
    public static class IServiceCollectionExtensions
    {
        public static void AddEmailGeneratorClient(this IServiceCollection services)
        {
            services.AddSingleton<EmailGenerator>();
            services.AddSingleton(s =>
            {
                var config = s.GetService<IOptions<EmailGeneratorConfiguration>>().Value;
                var channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
                var client = new EmailGeneratorClient(channel);
                return client;
            });
        }
    }
}
