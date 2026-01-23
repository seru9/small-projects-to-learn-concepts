using PcsSelcomWebLogger.Models;

namespace PcsSelcomWebLogger.Models
{
    public class AppLogsFilterViewModel
    {
        public List<AppLogs> Logs { get; set; } = new List<AppLogs>();

        public string SearchString { get; set; } = string.Empty;
        public string LogType { get; set; } = string.Empty;
        public DateTime? From { get; set; } // Co z NULL
        public DateTime? To { get; set; }
    }

}
