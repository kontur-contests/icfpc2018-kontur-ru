using System;

using log4net;
using log4net.Config;

namespace lib
{
    public class Log
    {
        public static ILog For<T>(T t)
        {
            return For(typeof(T));
        }

        public static ILog For(Type t)
        {
            return For(t.Name);
        }

        public static ILog For(string loggerName)
        {
            var  type = typeof(Log);
            var logRepository = LogManager.GetRepository(type.Assembly);
            XmlConfigurator.Configure(logRepository, type.Assembly.GetManifestResourceStream(type, "log4net.config"));
            return LogManager.GetLogger(type.Assembly, loggerName);
        }
    }
}