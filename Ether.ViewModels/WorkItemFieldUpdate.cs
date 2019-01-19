namespace Ether.ViewModels
{
    public class WorkItemFieldUpdate
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

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(OldValue) && string.IsNullOrEmpty(NewValue);
        }
    }
}
