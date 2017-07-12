using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.Enums
{
    public enum NotifyMessageType
    {
        None = 0,

        // success
        ScheduledSaved = 1,


        // failure
        ScheduledSavedFail = 1001
    }

    public static class NotifyMessageTypeExtensions
    {
        public static bool IsNone(this NotifyMessageType messageType)
        {
            return messageType == NotifyMessageType.None;
        }

        public static bool IsSuccess(this NotifyMessageType messageType)
        {
            return IsBetween(messageType, NotifyMessages.MinSuccess, NotifyMessages.MaxSuccess);
        }

        public static bool IsWarning(this NotifyMessageType messageType)
        {
            return IsBetween(messageType, NotifyMessages.MinWarning, NotifyMessages.MaxWarning);
        }

        public static bool IsError(this NotifyMessageType messageType)
        {
            return IsBetween(messageType, NotifyMessages.MinError, NotifyMessages.MaxError);
        }

        private static bool IsBetween(NotifyMessageType messageType, int min, int max)
        {
            return (int)messageType >= min && (int)messageType <= max;
        }
    }


    public static class NotifyMessages
    {
        internal const int MinSuccess = 1;
        internal const int MaxSuccess = 999;

        internal const int MinWarning = 1000;
        internal const int MaxWarning = 1999;

        internal const int MinError = 2000;
        internal const int MaxError = 2999;

        private static Dictionary<NotifyMessageType, string> messages = new Dictionary<NotifyMessageType, string>()
        {
            {NotifyMessageType.ScheduledSaved, "Schedule Saved" },
            {NotifyMessageType.ScheduledSavedFail, "Unable to save Schedule" }
        };

        public static string MessageFor(NotifyMessageType messageType)
        {
            if (messageType.IsNone())
                return "";

            if (messages.ContainsKey(messageType))
                return messages[messageType];
            return "Unknown notify message type: " + messageType;
        }

        public static string ClassFor(NotifyMessageType messageType, string prefix="")
        {
            if (messageType.IsNone())
                return "";

            if (messageType.IsSuccess())
                return prefix + "success";
            if (messageType.IsWarning())
                return prefix + "warning";
            if (messageType.IsError())
                return prefix + "error";
            return prefix + "unknown";
        }
    }
}
