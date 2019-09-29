using AutoMapper;
using Ether.Core.Config;
using Ether.Vsts.Config;
using NUnit.Framework;

namespace Ether.Tests.Classifiers
{
    public class AutoMapperTests
    {
        [Test]
        public void VerifyConfiguration()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new CoreMappingProfile());
                mc.AddProfile(new VstsMappingProfile());
            });

            mappingConfig.AssertConfigurationIsValid();
        }
    }
}
