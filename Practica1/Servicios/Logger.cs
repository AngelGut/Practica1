using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Servicios
{
    internal enum LogLevel { Info, Warn, Error }

    internal static class Logger
    {
        private static readonly object _sync = new();
        private static readonly string _logDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Practica1Logs");
        private static string ArchivoActual => Path.Combine(_logDir, $"app_{DateTime.Now:yyyyMMdd}.log");

        static Logger()
        {
            Directory.CreateDirectory(_logDir);
        }

        public static void Info(string msg, string? categoria = null) => Log(LogLevel.Info, msg, categoria);
        public static void Warn(string msg, string? categoria = null) => Log(LogLevel.Warn, msg, categoria);
        public static void Error(string msg, string? categoria = null, Exception? ex = null)
            => Log(LogLevel.Error, ex is null ? msg : $"{msg} | {ex.Message}\n{ex}");

        public static void Log(LogLevel level, string msg, string? categoria = null)
        {
            var linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
                        $"[{level}] " +
                        (string.IsNullOrWhiteSpace(categoria) ? "" : $"({categoria}) ") +
                        $"{msg}";
            lock (_sync)
            {
                File.AppendAllText(ArchivoActual, linea + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
