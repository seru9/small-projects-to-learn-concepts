using PcsSelcomWebLogger.Models;

namespace PcsSelcomWebLogger.Services
{
    public interface ILogBufferService
    {
        void Add(AppLogs log);
        List<AppLogs> GetAllAndClear();
    }

}
