using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace PcsSelcomWebLogger.Models
{
    public class AppLogs
    {
        public int Id { get; set; }
        [Display(Name = "App Name")]
        public string AppName { get; set; } = string.Empty;
        [Display(Name = "Date Time")]
        public DateTime DateTime { get; set; }
        [Display(Name = "Log Type")]
        public string LogType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
