using Ether.Core.Constants;
using Ether.Core.Models.DTO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ether.Core.Models.VSTS
{
    public class VSTSWorkItem : BaseDto
    {
        [JsonProperty("Id")]
        public int WorkItemId { get; set; }
        public string WorkItemType => this[VSTSFieldNames.WorkItemType];
        public string Title => this["System.Title"];
        public string AreaPath => this["System.AreaPath"];
        public string State => this["System.State"];
        public string Reason => this["System.Reason"];
        public DateTime? ChangedDate => GetDateOrDefault(this["System.ChangedDate"]);
        public DateTime? ResolvedDate => GetDateOrDefault(this["Microsoft.VSTS.Common.ResolvedDate"]);
        public DateTime? ClosedDate => GetDateOrDefault(this["Microsoft.VSTS.Common.ClosedDate"]);
        public DateTime? StateChangeDate => GetDateOrDefault(this["Microsoft.VSTS.Common.StateChangeDate"]);
        public DateTime? CreatedDate => GetDateOrDefault(this[VSTSFieldNames.WorkItemCreatedDate]);

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<string, string> Fields { get; set; }

        public string this[string key]
        {
            get
            {
                if (Fields == null || !Fields.ContainsKey(key))
                    return string.Empty;

                return Fields[key];
            }
        }

        public IEnumerable<WorkItemUpdate> Updates { get; set; }

        private DateTime? GetDateOrDefault(string value)
        {
            DateTime result;
            if (!DateTime.TryParse(value, out result))
                return null;

            return result;
        }
    }
}
