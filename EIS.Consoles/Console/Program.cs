using System;

namespace EIS.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			System.Console.WriteLine("Program started...");


			MainLoop loop = new MainLoop(5); // 5 seconds

			loop.StartAsync();


			System.Console.ReadLine();
			Environment.Exit(0);
		}
	}
}