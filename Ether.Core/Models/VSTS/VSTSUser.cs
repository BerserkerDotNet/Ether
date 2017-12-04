using System;

namespace Ether.Core.Models.VSTS
{
    public class VSTSUser : IEquatable<VSTSUser>
    {
        public bool IsContainer { get; set; }
        public string DisplayName { get; set; }
        public string UniqueName { get; set; }

        public bool Equals(VSTSUser other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(UniqueName, other.UniqueName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as VSTSUser);
        }

        public override int GetHashCode()
        {
            return UniqueName.GetHashCode();
        }
    }
}
