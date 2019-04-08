using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
namespace LinearProgrammingParser
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


		string equationPattern = @"([\+\-])*\s*\s*(\d\d*)*\w(\d\d*)";
		string minOrMaxPattern = @"(min|max)";
		string subjectToPattern = @"(s.t.|st|subject to)";
		string limitationsPattern = @"([<=>]+)\s*([\-\+]*\d)";


		public LinearParser(string pathToFile)
		{
			fileLines = File.ReadAllLines(pathToFile);
		}

		public void ParseFile()
		{
			if (SubjectToAndMinOrMaxExist())
				ParseFileByLine(0);
			else
				errorMsg = "You haven't entered minmax value or subject to keyword";

			MakeTxtFile();
		}

		/*
		 * Group 0 is the whole match. Group 1 is the sign, Group 2 is for the coefficient and Group 3 is for the index
		 */

		private void ParseFileByLine(int fileIndex)
		{
			int matchIndex;

			var equationCoefficientMatch = Regex.Match(fileLines[fileIndex], equationPattern);
			var limitationCoeffiecientsLine = new List<string>();
			var matchCount = 0;
			var usedCoefficientCount = 0;

			while (equationCoefficientMatch.Success)
			{
				var equationCoefficient = "";

				int.TryParse(equationCoefficientMatch.Groups[3].ToString(), out matchIndex);

				/*
				 * If the index of the variable is not the same as the match count then it means 
				 * that a variable was skipped in the eqaution, hence it has coefficient value of +0 since
				 * +0 * x = 0.
				 */
				while (matchCount != matchIndex - 1)
				{
					if (fileIndex == 0)
						equationCoefficients.Add("+0");
					else
						limitationCoeffiecientsLine.Add("+0");
					matchCount++;
				}

				var matchedSign = equationCoefficientMatch.Groups[1].ToString();
				var matchedCoefficient = equationCoefficientMatch.Groups[2].ToString();

				/* 
				 * Checks if the sign is empty. If it is then it checks if it is in the first variable of the
				 * equation. If the statement is true it assigns the + sign. If it's not then the program terminates
				 * displaying the correspondent message.
				 */
				if (string.IsNullOrEmpty(matchedSign))
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
					equationCoefficient += matchedSign;


				equationCoefficient += string.IsNullOrEmpty(matchedCoefficient) ? "1" : matchedCoefficient;

				/* 
				 * If we are in the first line of the equation then we append it to the minmax equation coeffiecients.
				 * If not, to the technology limitations coefficients.
				 */

				if (fileIndex == 0)
					equationCoefficients.Add(equationCoefficient);
				else
					limitationCoeffiecientsLine.Add(equationCoefficient);

				equationCoefficientMatch = equationCoefficientMatch.NextMatch();
				matchCount++;
				usedCoefficientCount++;
			}

			//If the fileIndex is greater than 0 then we are in the technology limitations section
			if (fileIndex > 0)
			{
				var technologyLimitationsMatch = Regex.Match(fileLines[fileIndex], limitationsPattern);

				var currentLimitationSign = technologyLimitationsMatch.Groups[1].ToString();

				/*
				 * Tests if the less-equal-more than sing exists and if it does it gives the currentLimitationSign 
				 * the right value. -1 for less than, 0 for equal and +1 for more-than
				 */

				if (string.IsNullOrEmpty(currentLimitationSign))
				{
					errorMsg = $"Missing less-equal-more than sign in line {fileIndex + 1}";
					return;
				}
				else
				{
					if (currentLimitationSign == "<=" || currentLimitationSign == "=<")
						technologyLimitationsSigns.Add("-1");
					else if (currentLimitationSign == "=")
						technologyLimitationsSigns.Add(" 0");
					else if (currentLimitationSign == ">=" || currentLimitationSign == "=>")
						technologyLimitationsSigns.Add("+1");
				}

				var currentLimitationValue = technologyLimitationsMatch.Groups[2].ToString();

				/*
				 * Checks if the value of the inequation is null and if the statement is true then the program terminates.
				 * If not it checks if it has a sign in front of it. If it hasn't it assigns it the + sign.
				 */

				if (string.IsNullOrEmpty(currentLimitationValue))
				{
					errorMsg = $"Missing limitation value in line {fileIndex + 1}";
					return;
				}
				else
					technologyLimitationsValues.Add(!(currentLimitationValue[0].Equals("+") || currentLimitationValue[0].Equals("-")) ?
						$"+{currentLimitationValue}" : currentLimitationValue);

				technologyLimitations.Add(limitationCoeffiecientsLine);
			}

			if (fileIndex < fileLines.Length - 2)
				ParseFileByLine(fileIndex + 1);
		}

		/*
		 * Tests if there is any minmax value in the first line and s.t or st or subject to in the second line. If both of them exist then 
		 * it saves the value of minmax value as -1 for min and 1 for max
		 */

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

		//Writes to file.
		private void MakeTxtFile()
		{

			using (var streamWriter = new StreamWriter(@"../../LP-02.txt"))
			{
				if (errorMsg != null)
				{
					streamWriter.WriteLine(errorMsg);
					return;
				}

				streamWriter.WriteLine($"MinMax = {minMax}, [{String.Join(", ", equationCoefficients)}] * x");
				for (int i = 0; i < technologyLimitations.Count; i++)
				{

					string line = $"{(i == 0 ? "s.t. " : new string(' ', 5))}[{String.Join(", ", technologyLimitations[i])}]" +
						$"{(i == technologyLimitations.Count / 2 ? " * x " : new string(' ', 5))}" +
						$"[Equin({i}) = {technologyLimitationsSigns[i]}] [{technologyLimitationsValues[i]}]";

					streamWriter.WriteLine(line);
				}

				streamWriter.WriteLine($"{new string(' ', 17)}{fileLines[fileLines.Length - 1].Trim()}");
			}
		}
	}
}
