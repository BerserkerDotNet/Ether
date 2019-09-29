using Newtonsoft.Json;

namespace Ether.Types
{
    public static class ObjectExtensions
    {
        public static T PoorMansClone<T>(this T original)
        {
            var json = JsonConvert.SerializeObject(original);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
