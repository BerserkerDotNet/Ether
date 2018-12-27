using System;

namespace Ether.ViewModels
{
    public class ViewModelWithId : IEquatable<ViewModelWithId>
    {
        public Guid Id { get; set; }

        public bool Equals(ViewModelWithId other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ViewModelWithId);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
