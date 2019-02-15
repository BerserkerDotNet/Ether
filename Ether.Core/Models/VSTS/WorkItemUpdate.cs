using Ether.Core.Constants;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Ether.Core.Models.VSTS
{
    public class WorkItemUpdate
    {
        public DateTime RevisedDate { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<string, UpdateValue> Fields { get; set; }

        public RelationsContainer Relations { get; set; }

        public UpdateValue Reason => this["System.Reason"];
        public UpdateValue AreaPath => this["System.AreaPath"];
        public UpdateValue ResolvedBy => this["Microsoft.VSTS.Common.ResolvedBy"];
        public UpdateValue ClosedBy => this[VSTSFieldNames.ClosedBy];
        public UpdateValue AssignedTo => this[VSTSFieldNames.AssignedTo];
        public UpdateValue State => this["System.State"];
        public UpdateValue WorkItemType => this["System.WorkItemType"];
        public UpdateValue Title => this["System.Title"];
        public UpdateValue Tags => this[VSTSFieldNames.Tags];
        public DateTime ChangedDate => DateTime.Parse(this[VSTSFieldNames.ChangedDate].NewValue);
        public bool HasChangedDate => Fields.ContainsKey(VSTSFieldNames.ChangedDate);

        public bool CanBeDiscarded => Reason.IsEmpty
                                      && AreaPath.IsEmpty
                                      && ResolvedBy.IsEmpty
                                      && ClosedBy.IsEmpty
                                      && AssignedTo.IsEmpty
                                      && State.IsEmpty
                                      && Tags.IsEmpty
                                      && (Relations?.Added == null || !Relations.Added.Any(i => i.IsPullRequest));

        public UpdateValue this[string key]
        {
            get
            {
                if (Fields == null || !Fields.ContainsKey(key))
                    return new UpdateValue();

                return Fields[key];
            }
        }

        public class UpdateValue
        {
            public string NewValue { get; set; }
            public string OldValue { get; set; }

            public bool IsValueChanged()
            {
                return !string.Equals(NewValue, OldValue);
            }

            public bool IsValueCleared()
            {
                return !string.IsNullOrEmpty(OldValue) && string.IsNullOrEmpty(NewValue);
            }

            public bool IsEmpty => string.IsNullOrEmpty(OldValue) && string.IsNullOrEmpty(NewValue);
        }

        public class RelationsContainer
        {
            public List<RelationItem> Added { get; set; }
            public List<RelationItem> Removed { get; set; }
        }

        public class RelationItem
        {
            [JsonProperty("rel")]
            public string Relation { get; set; }
            public string Url { get; set; }
            public string Name => Attributes != null && Attributes.ContainsKey("name") ? Attributes["name"] : null;

            public bool IsPullRequest => !string.IsNullOrWhiteSpace(Name) && Name.Equals("Pull Request", StringComparison.InvariantCultureIgnoreCase);

            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public Dictionary<string, string> Attributes { get; set; }
        }
    }
}
