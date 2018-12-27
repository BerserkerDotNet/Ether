using System.Collections.Generic;
using Ether.Contracts.Interfaces;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using VSTS.Net.Models.WorkItems;

namespace Ether.Vsts.Config
{
    public class VstsClassMapRegistration : IClassMapRegistration
    {
        public void Register()
        {
            BsonClassMap.RegisterClassMap<WorkItem>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Fields).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, string>>(DictionaryRepresentation.ArrayOfDocuments));
            });

            BsonClassMap.RegisterClassMap<WorkItemUpdate>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Fields).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, WorkItemFieldUpdate>>(DictionaryRepresentation.ArrayOfDocuments));
            });
        }
    }
}
