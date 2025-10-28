using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WG.PDFToolkit.Logger
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly string _categoryName;

        public FileLogger(string categoryName, string filePath)
        {
            _categoryName = categoryName;
            _filePath = filePath;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception?, string> formatter)
        {
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
            File.AppendAllText(_filePath, message + Environment.NewLine);
        }
    }
}
