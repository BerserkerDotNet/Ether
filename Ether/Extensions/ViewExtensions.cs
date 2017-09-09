using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ether.Extensions
{
    public static class TempDataExtensions
    {
        public const string NotificationData = "NotificationData";
        public const string NotificationError = "Error";
        public const string NotificationSuccess = "Success";
        public const string NotificationWarning = "Warning";

        public static void WithSuccess(this ITempDataDictionary data, string message)
        {
            AddMessage(data, NotificationSuccess, message);
        }

        public static void WithWarning(this ITempDataDictionary data, string message)
        {
            AddMessage(data, NotificationWarning, message);
        }

        public static void WithError(this ITempDataDictionary data, string message)
        {
            AddMessage(data, NotificationError, message);
        }

        private static void AddMessage(ITempDataDictionary data, string type, string message)
        {
            if (!data.ContainsKey(NotificationData))
                data.Add(NotificationData, $"{type}|{message}");
        }
    }
}
