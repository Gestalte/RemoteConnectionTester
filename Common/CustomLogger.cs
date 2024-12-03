using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;

namespace Common
{
    public static class LogWriter
    {
        public static Action<string>? WriteLogAction { get; set; }

        public static string WriteLog(string message)
        {
            WriteLogAction?.Invoke(message);

            System.Diagnostics.Debug.WriteLine(message);

            return message;
        }
    }

    public sealed class CustomLogger : ActorBase, IRequiresMessageQueue<ILoggerMessageQueueSemantics>
    {
        protected override bool Receive(object message)
        {
            if (message is InitializeLogger)
            {
                Sender.Tell(new LoggerInitialized());
                return true;
            }

            if (message is not LogEvent logEvent)
            {
                return false;
            }

            Print(logEvent);
            return true;
        }

        /// <summary>
        /// Print the specified log event.
        /// </summary>
        /// <param name="logEvent">The log event that is to be output.</param>
        private static void Print(LogEvent logEvent)
        {
            LogWriter.WriteLog(logEvent.ToString());
        }
    }
}
