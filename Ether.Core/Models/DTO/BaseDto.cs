using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Ether.Core.Models.DTO
{
    public class BaseDto: IEquatable<BaseDto>
    {
        public Guid Id { get; set; }

        public bool Equals(BaseDto other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BaseDto;
            if (other == null)
                return false;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
