namespace Ether.Core.Models.VSTS.Response
{
    public class ValueBasedResponse<T>
    {
        public int Count { get; set; }
        public T[] Value { get; set; }
    }
}
