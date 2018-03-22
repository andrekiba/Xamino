using System;
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

            if (!string.IsNullOrWhiteSpace(error))
            {
                var message = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(exceptionHeader))
                    message.AppendLine(string.Concat(exceptionHeader, ":"));
                message.AppendLine(error);
                Debug.WriteLine(error);
                Debug.WriteLine(stackTrace);
                await UserDialogs.Instance.AlertAsync(message.ToString(), "Errore", "OK");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(successMessage))
                await UserDialogs.Instance.AlertAsync(successMessage, "Conferma", "OK");
            return true;
        }
    }
}
