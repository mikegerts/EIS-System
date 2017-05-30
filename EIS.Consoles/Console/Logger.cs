using System;
using log4net;
using log4net.Config;

namespace EIS.Console
{
	public static class Logger
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof (Logger));

		static Logger()
		{
			XmlConfigurator.Configure();
		}

		public static void LogDisplay(object msg, string type)
		{
			System.Console.WriteLine(msg);

			if(type == "Info")
				LogInfo(msg.ToString());
			else if(type == "Error")
				LogError(msg.ToString());
			else if(type == "Warning")
				LogWarning(msg.ToString());
			else
				LogException((Exception)msg);
		}

		public static void LogInfo(string msg)
		{
			logger.Info(msg);
		}

		public static void LogError(string msg)
		{
			logger.Error(msg);
		}

		public static void LogWarning(string msg)
		{
			logger.Warn(msg);
		}

		public static void LogException(Exception ex)
		{
			logger.Error(ex);
		}
	}
}
