using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace LinearProgrammingProblemParser
{
	class LinearParser
	{
		private string[] fileLines;
		private string minMax;
		private string errorMsg;
		private List<List<string>> technologyLimitations = new List<List<string>>();
		private List<string> technologyLimitationsSigns = new List<string>();
		private List<string> technologyLimitationsValues = new List<string>();
		private List<string> equationCoefficients = new List<string>();


		string equationPattern = @"([\+\-*/])*\s*\s*(\d\d*)*\w(\d\d*)[^\s]*";
		string minOrMaxPattern = @"(min|max)";
		string subjectToPattern = @"(s.t.|st|subject to)";
		string limitationsPattern = @"([<=>]+)\s*([\-\+]*\d)";


		public LinearParser(string pathToFile)
		{
			fileLines = System.IO.File.ReadAllLines(pathToFile);
		}

		public void ParseFile()
		{
			if (SubjectToAndMinOrMaxExist())
			{
				ParseFileByLine(0);
				if (errorMsg == null)
				{
					Console.WriteLine("Technology Limitations: ");
					technologyLimitations.ForEach((List<string> coefficientList) =>
					{
						Console.WriteLine(String.Join(", ", coefficientList.ToArray()));
					});
					Console.WriteLine($"MinMax: {minMax}");
					Console.WriteLine("EquationCoefficients");
					Console.WriteLine(String.Join(", ", equationCoefficients.ToArray()));
				}
				else
					Console.WriteLine(errorMsg);

			}
			else
			{
				Console.WriteLine("You haven't entered minmax value or subject to keyword");
				return;
			}
		}

		/*
		 * Group 0 is the whole match. Group 1 is the sign, Group 2 is for the coefficient and Group 3 is for the index
		 */

		private void ParseFileByLine(int fileIndex)
		{
			int matchIndex;

			//Console.WriteLine($"File index : {fileIndex}");
			//Console.WriteLine($"File length : {fileLines.Length}");
			var equationCoefficientMatch = Regex.Match(fileLines[fileIndex], equationPattern);
			var limitationCoeffiecientsLine = new List<string>();
			var matchCount = 0;
			var usedCoefficientCount = 0;

			while (equationCoefficientMatch.Success)
			{
				var equationCoefficient = "";

				int.TryParse(equationCoefficientMatch.Groups[3].ToString(), out matchIndex);


				while (matchCount != matchIndex - 1)
				{
					if (fileIndex == 0)
						equationCoefficients.Add("+0");
					else
						limitationCoeffiecientsLine.Add("+0");
					matchCount++;
				}


				for (int i = 1; i < 4; i++)
				{
					var selectedGroup = equationCoefficientMatch.Groups[i].ToString();
					if (i == 1)
					{
						if (string.IsNullOrEmpty(selectedGroup))
						{
							if (usedCoefficientCount == 0)
								equationCoefficient += "+";
							else
							{
								errorMsg = $"Missing a sign at line {fileIndex}";
								return;
							}
						}
						else
							equationCoefficient += selectedGroup;
					}

					if (i == 2)
						equationCoefficient += string.IsNullOrEmpty(selectedGroup) ? "1" : selectedGroup;

				}
				if (fileIndex == 0)
					equationCoefficients.Add(equationCoefficient);
				else
					limitationCoeffiecientsLine.Add(equationCoefficient);

				equationCoefficientMatch = equationCoefficientMatch.NextMatch();
				matchCount++;
				usedCoefficientCount++;
			}

			if (fileIndex > 0)
			{
				var technologyLimitationsMatch = Regex.Match(fileLines[fileIndex], limitationsPattern);

				var currentLimitationSign = technologyLimitationsMatch.Groups[1].ToString();
				if (string.IsNullOrEmpty(currentLimitationSign))
				{
					errorMsg = $"Missing less-equal-more than sign in line {fileIndex}";
					return;
				}
				else
				{
					if (currentLimitationSign == "<=" || currentLimitationSign == "=<")
						technologyLimitationsSigns.Add("-1");
					else if (currentLimitationSign == "=")
						technologyLimitationsSigns.Add("0");
					else if (currentLimitationSign == ">=" || currentLimitationSign == "=>")
						technologyLimitationsSigns.Add("1");
				}

				var currentLimitationValue = technologyLimitationsMatch.Groups[2].ToString();

				if (string.IsNullOrEmpty(currentLimitationValue))
				{
					errorMsg = $"Missing limitation value in line {fileIndex}";
					return;
				}
				else
					technologyLimitationsValues.Add(currentLimitationValue);
			}

			technologyLimitations.Add(limitationCoeffiecientsLine);
			if (fileIndex < fileLines.Length - 2)
				ParseFileByLine(fileIndex + 1);

		}

		private bool SubjectToAndMinOrMaxExist()
		{
			var minMaxRegex = new Regex(minOrMaxPattern);
			var subjectToRegex = new Regex(subjectToPattern);

			if (!(minMaxRegex.IsMatch(fileLines[0]) && subjectToRegex.IsMatch(fileLines[1])))
				return false;

			var minMaxMatch = minMaxRegex.Match(fileLines[0]);
			minMax = minMaxMatch.Groups[0].ToString() == "min" ? "-1" : "1";
			return true;
		}
	}
}
