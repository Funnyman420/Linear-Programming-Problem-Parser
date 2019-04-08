using System;

namespace LinearProgrammingParser
{
	class Program
	{
		static void Main(string[] args)
		{
			var problemPath = @"../../LP-01.txt";
			var linearParser = new LinearParser(problemPath);
			linearParser.ParseFile();
			Console.ReadKey();
		}
	}
}
