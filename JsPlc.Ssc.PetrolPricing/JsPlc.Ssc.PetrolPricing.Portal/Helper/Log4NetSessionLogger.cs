using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper
{
    public class Log4NetSessionLogger
    {
        private HttpSessionStateBase session;

        public Log4NetSessionLogger(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public void Error(Exception ex)
        {
            AppendMessage("Error at " + DateTime.Now + " - Exception: " + ex.ToString());
        }
        public void Information(string message)
        {
            AppendMessage("Information at " + DateTime.Now + " - Message: " + message);
        }

        public void Clear()
        {
            this.session["Log4Net"] = "";
        }

        public string GetLogText()
        {
            var log = this.session["Log4Net"];
            return log == null ? String.Empty : log.ToString();
        }

        private void AppendMessage(string message)
        {
            var sb = new StringBuilder(GetLogText());
            sb.AppendLine(message);
            this.session["Log4Net"] = sb.ToString();
        }

    }
}