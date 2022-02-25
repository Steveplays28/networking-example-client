#if GODOT
using Godot;
#elif NETFRAMEWORK
using System;
#endif

namespace SteveNetworking
{
	public static class LogHelper
	{
		public static string LogHeaderClient = "[Client]: ";
		public static string LogHeaderServer = "[Server]: ";

		/// <summary>
		/// Logs an info message to the console/debugger, depending on the environment.
		/// </summary>
		/// <param name="isClient">Decides wether to use the client or server log header.</param>
		/// <param name="message">The message that will be logged to the console.</param>
		public static void Log(bool isClient, string message)
		{
			string logHeader = isClient ? LogHeaderClient : LogHeaderServer;
			string logOutput = string.Concat(logHeader, message);

#if GODOT
			GD.Print(logOutput);
#elif NETFRAMEWORK
		Console.WriteLine(logOutput);
#endif
		}

		/// <summary>
		/// Logs a warning message to the console/debugger, depending on the environment.
		/// </summary>
		/// <param name="isClient">Decides wether to use the client or server log header.</param>
		/// <param name="message">The warning message that will be logged to the console.</param>
		public static void LogWarning(bool isClient, string message)
		{
			string logHeader = isClient ? LogHeaderClient : LogHeaderServer;
			string logOutput = string.Concat(logHeader, message);

#if GODOT
			GD.PushWarning(logOutput);
#elif NETFRAMEWORK
		Console.WriteLine(string.Concat("WARNING - ", logOutput));
#endif
		}

		/// <summary>
		/// Logs an error message to the console/debugger, depending on the environment.
		/// </summary>
		/// <param name="isClient">Decides wether to use the client or server log header.</param>
		/// <param name="message">The error message that will be logged to the console.</param>
		public static void LogError(bool isClient, string message)
		{
			string logHeader = isClient ? LogHeaderClient : LogHeaderServer;
			string logOutput = string.Concat(logHeader, message);

#if GODOT
			GD.PushError(logOutput);
#elif NETFRAMEWORK
		Console.Error.WriteLine(logOutput);
#endif
		}
	}
}
