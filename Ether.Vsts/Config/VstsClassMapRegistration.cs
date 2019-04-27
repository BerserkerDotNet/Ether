using System.Collections.Generic;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Ether.Vsts.Config
{
    public class VstsClassMapRegistration : IClassMapRegistration
    {
        public void Register()
        {
            /* Disabled until https://github.com/mongodb/mongo-csharp-driver/pull/372 is checked in
            BsonClassMap.RegisterClassMap<WorkItem>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Fields).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, string>>(DictionaryRepresentation.ArrayOfDocuments));
            });

            BsonClassMap.RegisterClassMap<WorkItemUpdateViewModel>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Fields).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, WorkItemFieldUpdate>>(DictionaryRepresentation.ArrayOfDocuments));
            });

            BsonClassMap.RegisterClassMap<WorkItemFieldUpdate>();*/
        }
    }
}
