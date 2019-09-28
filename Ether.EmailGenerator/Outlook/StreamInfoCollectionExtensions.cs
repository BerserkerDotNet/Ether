using System.IO.Packaging;
using System.Linq;

namespace Ether.EmailGenerator.Outlook
{
    public static class StreamInfoCollectionExtensions
    {
        public const string SubjectStreamTag = "0037";
        public const string SubjectNormilizedStreamTag = "0E1D";
        public const string BodyRTFStreamTag = "1009";

        public static StreamInfo GetRTFBodyStream(this StreamInfo[] streams)
        {
            return GetPropertyStream(streams, BodyRTFStreamTag);
        }

        public static StreamInfo GetSubjectStream(this StreamInfo[] streams)
        {
            return GetPropertyStream(streams, SubjectStreamTag);
        }

        public static StreamInfo GetSubjectNormilizedStream(this StreamInfo[] streams)
        {
            return GetPropertyStream(streams, SubjectNormilizedStreamTag);
        }

        public static StreamInfo GetPropertyStream(this StreamInfo[] streams, string tag)
        {
            return streams.Single(s => s.Name.StartsWith($"__substg1.0_{tag}"));
        }

        public static StreamInfo GetPropertiesStream(this StreamInfo[] streams)
        {
            return streams.Single(s => s.Name.StartsWith("__properties_"));
        }
    }
}
