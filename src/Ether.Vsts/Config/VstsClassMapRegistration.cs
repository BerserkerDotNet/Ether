using System.Collections.Generic;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.ViewModels.Types;
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
            SafeRegisterClassMap<PullRequestsReport>();
            SafeRegisterClassMap<AggregatedWorkitemsETAReport>();
            SafeRegisterClassMap<WorkItemsReport>();
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

            BsonClassMap.RegisterClassMap<WorkItemFieldUpdate>();
            BsonClassMap.RegisterClassMap<PullRequestJobDetails>();
        }

        private void SafeRegisterClassMap<T>()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>();
            }
        }
    }
}
