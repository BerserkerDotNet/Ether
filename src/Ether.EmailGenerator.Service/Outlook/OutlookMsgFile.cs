using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using MarkupConverter;

namespace Ether.EmailGenerator.Outlook
{
    public class OutlookMsgFile
    {
        // {Type:2}{Tag:2}{Flags:4}{Size:4}
        // https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxmsg/bac41dfb-c824-4e3c-9b5e-b61106f6739f
        private const int StreamSizeOffset = 6;

        private OutlookMsgFile(string path)
        {
            FilePath = path;
        }

        public string FilePath { get; }

        public static OutlookMsgFile New(string id)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "EtherEmailGenerator");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            var filePath = Path.Combine(tempDir, $"{id}.msg");

            // File.WriteAllBytes(filePath, Resources.MsgTemplate);

            return new OutlookMsgFile(filePath);
        }

        public void SetSubjectAndBody(string subject, string htmlBody)
        {
            var storageRoot = OpenCompoundFile(FilePath);
            try
            {
                var streams = storageRoot.GetStreams();

                WriteSubject(streams, subject);
                WriteBody(streams, htmlBody);
            }
            finally
            {
                CloseCompoundFile(storageRoot);
            }
        }

        private void WriteBody(StreamInfo[] streams, string htmlBody)
        {
            var propertiesStream = streams.GetPropertiesStream();
            var bodyStream = streams.GetRTFBodyStream();

            var rtfBody = HtmlToRtfConverter.ConvertHtmlToRtf(htmlBody);
            var bodyBytes = Encoding.ASCII.GetBytes(rtfBody);
            var bodyAndHeaderLength = BitConverter.GetBytes(bodyBytes.Length + 12); // 12 - {UncopressedSize:4}{Magic:4}{CRC32:4}
            var bodyLength = BitConverter.GetBytes(bodyBytes.Length);
            var crc32 = CRC32.CalculateCRC32(bodyBytes);

            var bodyData = new List<byte>();
            bodyData.AddRange(bodyAndHeaderLength);
            bodyData.AddRange(bodyLength);
            bodyData.AddRange(new byte[] { 0x4D, 0x45, 0x4C, 0x41 }); // Magic number (0x414c454d) means RTF is not compressed
            bodyData.AddRange(BitConverter.GetBytes(crc32));
            bodyData.AddRange(bodyBytes);
            var bodyDataBytes = bodyData.ToArray();
            WriteBytesAndTruncate(bodyStream, bodyDataBytes);

            // Update stream info in properties
            var properties = GetBytes(propertiesStream);
            var bodyIndex = properties.IndexOf(new byte[] { 0x09, 0x10 }); // BodyRTF tag
            if (bodyIndex == -1)
            {
                throw new ArgumentException($"Could not locate body index in properties stream. BIdx: {bodyIndex};");
            }

            var fullBodyLength = BitConverter.GetBytes(bodyDataBytes.Length);
            WriteBytes(propertiesStream, fullBodyLength, bodyIndex + StreamSizeOffset);
        }

        private void WriteSubject(StreamInfo[] streams, string subject)
        {
            var propertiesStream = streams.GetPropertiesStream();
            var subjectStream = streams.GetSubjectStream();
            var subjectNormilizedStream = streams.GetSubjectNormilizedStream();

            var subjectBytes = Encoding.Unicode.GetBytes(subject);

            /*
             * PtypString https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxmsg/bac41dfb-c824-4e3c-9b5e-b61106f6739f
             * Size MUST be equal to 2 plus the size of the stream where the value of the property represented by this entry is stored.
             * The string being stored MUST<2> have at least one character.
             * When parsing property streams, clients MUST issue a MAPI_E_BAD_VALUE error for any zero-length property streams of PtypString.
             */
            var subjectLength = BitConverter.GetBytes(subjectBytes.Length + 2);

            WriteBytesAndTruncate(subjectStream, subjectBytes);
            WriteBytesAndTruncate(subjectNormilizedStream, subjectBytes);

            // Update stream info in properties
            var properties = GetBytes(propertiesStream);
            var subjectIndex = properties.IndexOf(new byte[] { 0x37, 0x00 }); // Subject Tag
            var subjectNormilizedIndex = properties.IndexOf(new byte[] { 0x1D, 0x0E }); // Subject Normilized Tag

            if (subjectNormilizedIndex == -1 || subjectIndex == -1)
            {
                throw new ArgumentException($"Could not locate subject index in properties stream. SIdx: {subjectIndex}; SNIdx: {subjectNormilizedIndex}");
            }

            // Set size info for both subject properties
            WriteBytes(propertiesStream, subjectLength, subjectIndex + StreamSizeOffset);
            WriteBytes(propertiesStream, subjectLength, subjectNormilizedIndex + StreamSizeOffset);
        }

        private void WriteBytes(StreamInfo streamInfo, byte[] data, int offset)
        {
            using (var dataStream = streamInfo.GetStream())
            {
                dataStream.Seek(offset, SeekOrigin.Begin);
                dataStream.Write(data, 0, data.Length);
            }
        }

        private void WriteBytesAndTruncate(StreamInfo streamInfo, byte[] data)
        {
            using (var dataStream = streamInfo.GetStream())
            {
                dataStream.Write(data, 0, data.Length);
                dataStream.SetLength(data.Length);
            }
        }

        private byte[] GetBytes(StreamInfo streamInfo)
        {
            using (var dataStream = streamInfo.GetStream())
            {
                var data = new byte[dataStream.Length];
                dataStream.Read(data, 0, data.Length);
                return data;
            }
        }

        private StorageInfo OpenCompoundFile(string path)
        {
            // We need the StorageRoot class to directly open an OSS file.  Unfortunately, it's internal.
            // So we'll have to use Reflection to access it.  This code was inspired by:
            // http://henbo.spaces.live.com/blog/cns!2E073207A544E12!200.entry
            // Note: In early WinFX CTPs the StorageRoot class was public because it was documented
            // here: http://msdn2.microsoft.com/en-us/library/aa480157.aspx

            var storageRootType = typeof(StorageInfo).Assembly.GetType("System.IO.Packaging.StorageRoot", true, false);
            var bindings = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            var result = storageRootType.InvokeMember("Open", bindings, null, null, new object[] { path, FileMode.Open, FileAccess.ReadWrite, FileShare.None });
            return (StorageInfo)result;
        }

        private void CloseCompoundFile(StorageInfo root)
        {
            var storageRootType = typeof(StorageInfo).Assembly.GetType("System.IO.Packaging.StorageRoot", true, false);
            var bindings = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            storageRootType.InvokeMember("Close", bindings, null, root, new object[0]);
        }
    }
}
