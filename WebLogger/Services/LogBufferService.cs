using PcsSelcomWebLogger.Models;

namespace PcsSelcomWebLogger.Services
{
    public class LogBufferService : ILogBufferService
    {
        private List<AppLogs> _buffer = new();
        private readonly object _lock = new();

        public void Add(AppLogs log)
        {
            lock (_lock)
            {
                _buffer.Add(log);
            }
        }
        public List<AppLogs> GetAllAndClear()
        {
            lock (_lock)
            {
                var logsToReturn = _buffer.ToList();
                _buffer.Clear();
                return logsToReturn;
            }
        }
    }

}
