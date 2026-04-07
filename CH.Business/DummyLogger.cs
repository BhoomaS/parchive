using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CH.Business
{
    // TODO: Create an actual logger class, possibly a "multi-logger"
    // that writes to Windows Event Log, file, application database, etc...
    public class DummyLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Dummy logger, do nothing.
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Dummy logger, do nothing.
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Dummy logger, do nothing.
            throw new NotImplementedException("Dummy logger BeginScope not implemented");
        }
    }
}
