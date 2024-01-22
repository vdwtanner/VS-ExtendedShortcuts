using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace ExtendedShortcuts
{
    internal class Logger
    {
        public enum Severity
        {
            Critical = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
        }

        private static Logger Instance { get; set; }
        
        
        private OutputWindowPane outputPane;
        public static Severity logSeverityFilter { get; set; }
        public static Severity msgBoxSeverityFilter { get; set; }
        public static Severity activateOutputPaneSeverityFilter { get; set; }

        private Logger(OutputWindowPane pane)
        {
            outputPane = pane;

            //set defaults
            logSeverityFilter = Severity.Info;
            msgBoxSeverityFilter = Severity.Error;
            activateOutputPaneSeverityFilter = Severity.Warning;
        }

        public static async Task InitAsync(string name)
        {
            if(Instance != null)
            {
                throw new InvalidOperationException("Logger instance already exists");
            }

            var pane = await VS.Windows.CreateOutputWindowPaneAsync(name, false);
            if(pane == null)
            {
                await VS.MessageBox.ShowErrorAsync("ExtendedShortcuts", "Failed to initialize output pane. Calls will be unsuccessful.");
            }

            Instance = new Logger(pane);
        }

        public static async Task LogAsync(string message, Severity severity = Severity.Info)
        {
            if (severity <= logSeverityFilter && Instance.outputPane != null)
            {
                string logMsg = $"[{severity}]  {message}";
                await Instance.outputPane.WriteLineAsync(logMsg);

                // check if we should set our output pane as active. Note that this doesn't actually force the output window to become visible!
                if (severity <= activateOutputPaneSeverityFilter)
                {
                    await Instance.outputPane.ActivateAsync();
                }
            }
            if(severity <= msgBoxSeverityFilter)
            {
                switch (severity)
                {
                    case Severity.Info: //fallthrough
                    case Severity.Warning: await VS.MessageBox.ShowWarningAsync(message); break;
                    case Severity.Error: //fallthrough
                    case Severity.Critical: await VS.MessageBox.ShowErrorAsync(message); break;
                }
            }
            
        }
    }
}
