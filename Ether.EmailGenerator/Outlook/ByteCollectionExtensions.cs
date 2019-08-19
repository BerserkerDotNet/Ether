using System.Linq;

namespace Ether.EmailGenerator.Outlook
{
    public static class ByteCollectionExtensions
    {
        public static int IndexOf(this byte[] array, byte[] pattern)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var isFound = array.Skip(i).Take(pattern.Length).SequenceEqual(pattern);
                if (isFound)
                {
                    return i;
                }

                if (i + pattern.Length == array.Length)
                {
                    return -1;
                }
            }

            return -1;
        }
    }
}
