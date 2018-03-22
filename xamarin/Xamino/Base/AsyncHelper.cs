using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;

namespace Xamino.Base
{
    public static class AsyncHelper
    {
        public static async Task<bool> DoAction(Func<Task> action, string exceptionHeader = null, string successMessage = null)
        {
            string error = null;
            string stackTrace = null;

            try
            {
                await action();
            }
            catch (OperationCanceledException)
            {
                error = "Time out!";
            }
            catch (Exception e)
            {
                error = e.Message;
                stackTrace = e.StackTrace;
            }

            if (!error.IsNullOrWhiteSpace())
            {
                var message = new StringBuilder();
                if (!exceptionHeader.IsNullOrWhiteSpace())
                    message.AppendLine(string.Concat(exceptionHeader, ":"));
                message.AppendLine(error);
                Debug.WriteLine(error);
                Debug.WriteLine(stackTrace);
                await UserDialogs.Instance.AlertAsync(message.ToString(), "Errore", "OK");
                return false;
            }

            if (!successMessage.IsNullOrWhiteSpace())
                await UserDialogs.Instance.AlertAsync(successMessage, "Conferma", "OK");
            return true;
        }
    }
}
