using System.Collections.Generic;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Ether.Core.Config
{
    public class CoreClassMapRegistration : IClassMapRegistration
    {
        public void Register()
        {
            BsonClassMap.RegisterClassMap<ReOpenedWorkItemsReport>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.ResolvedWorkItemsLookup).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, int>>(DictionaryRepresentation.ArrayOfDocuments));
                cm.MapMember(c => c.MembersLookup).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, string>>(DictionaryRepresentation.ArrayOfDocuments));
            });
        }
    }
}
