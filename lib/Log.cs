using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using log4net;
using log4net.Config;

namespace lib
{
    public class Log
    {
        public static ILog For<T>()
        {
            return For(typeof(T));
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public static ILog For<T>(T t)
        {
            return For(typeof(T));
        }

        public static ILog For(Type t)
        {
            return For(t.FullName);
        }

        public static ILog For(string loggerName)
        {
            return LoggerRepository.GetLogger(loggerName);
        }

        private static class LoggerRepository
        {
            private static readonly Assembly thisAssembly = Assembly.GetAssembly(typeof(Log));
            private static readonly ConcurrentDictionary<string, ILog> loggers = new ConcurrentDictionary<string, ILog>();

            static LoggerRepository()
            {
                var logRepository = LogManager.GetRepository(thisAssembly);
                using (var rs = thisAssembly.GetManifestResourceStream(typeof(Log), "log4net.config"))
                    XmlConfigurator.Configure(logRepository, rs);
            }

            public static ILog GetLogger(string loggerName)
            {
                return loggers.GetOrAdd(loggerName, t => LogManager.GetLogger(thisAssembly, loggerName));
            }
        }
    }
}