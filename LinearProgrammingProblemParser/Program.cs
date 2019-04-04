using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgrammingProblemParser
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
