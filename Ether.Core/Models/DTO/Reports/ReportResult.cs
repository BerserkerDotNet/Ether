using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Ether.Core.Models.DTO.Reports
{
    [BsonIgnoreExtraElements]
    public class ReportResult : BaseDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProfileName { get; set; }
        public DateTime DateTaken { get; set; }
        public Guid ReporterId { get; set; }
        public string ReportName { get; set; }
        public TimeSpan? GeneratedIn { get; set; }
    }
}
