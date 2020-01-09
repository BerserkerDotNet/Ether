namespace Ether.Types
{
    public class SelectOption<T>
    {
        public SelectOption(T value, string name)
        {
            Value = value;
            Name = name;
        }

        public T Value { get; set; }

        public string Name { get; set; }
    }
}
