using System.Collections.Generic;
using Ether.Contracts.Dto;
using Ether.ViewModels;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Ether.Vsts.Dto
{
    public class WorkItemUpdate : BaseDto
    {
        public int UpdateId { get; set; }

        public int WorkItemId { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<string, WorkItemFieldUpdate> Fields { get; set; }
    }
}
