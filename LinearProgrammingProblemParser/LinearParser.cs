using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LinearProgrammingProblemParser
{
	class LinearParser
	{
		private string[] fileLines;
		List<TechnologyLimitation> technologyLimitations;
		List<string> equationCoefficients = new List<string>();
		private string typeOfProblem;

		string equationPattern = @"([\+\-*/])*\s*(\d\d*)*\w(\d\d*)[^\s]*";
		string minOrMaxPattern = @"(min|max)";
		string subjectToPattern = @"(s.t.|st|subject to)";
		string limitationsPattern = @"([<=>]+)\s*(\d)";


		public LinearParser(string pathToFile)
		{
			fileLines = System.IO.File.ReadAllLines(pathToFile);
		}

		public void ParseFile()
		{

			
		}

		private bool CheckForNoneExistantVariable(string groupValue, int counter)
		{
			int variableIndex;
			Int32.TryParse(groupValue, out variableIndex);
			if (variableIndex == counter + 1)
				return false;
			else
				return true;
		}

		private bool SubjectToAndMinOrMaxExist()
		{
			var parseFlag = true;

			Match minOrMaxMatch = Regex.Match(fileLines[0], minOrMaxPattern);
			var minOrMax = minOrMaxMatch.Groups[0].ToString();

			Match subjectToMatch = Regex.Match(fileLines[1], subjectToPattern);
			var subjectTo = subjectToMatch.Groups[0].ToString();

			if (string.IsNullOrEmpty(minOrMax) || string.IsNullOrEmpty(subjectTo))
				parseFlag = false;
			else
				typeOfProblem = minOrMax;



			return parseFlag;
		}

		private List<string> GetEquationCoefficients(int fileIndex)
		{
			var coefficients = new List<string>();
			var matchCount = 0;
			var equationMatch = Regex.Match(fileLines[fileIndex], equationPattern);
			while (equationMatch.Success)
			{
				string sign = "";
				string coefficient = "";
				string joinedCoefficient = "";

				var groups = equationMatch.Groups;
				for (var i = 1; i < groups.Count; i++)
				{
					string groupValue = groups[i].ToString();
					if (fileIndex != 0)
					{
						if (CheckForNoneExistantVariable(groupValue, matchCount))
						{
							int counter = matchCount;
							int valueToReach;
							Int32.TryParse(groupValue, out valueToReach);
							while (true)
							{
								if(counter != valueToReach)
								{
									coefficients.Add("+0");
								}
							}
						}
					}
					if (i == 1)
					{
						if (string.IsNullOrWhiteSpace(groupValue))
						{
							if (matchCount == 0)
								sign = "+";
							else
							{
								Console.WriteLine($"Variable {matchCount + 1} from equation doensn't have a sign");
								throw new Exception();
							}
						}
						else
							sign = groupValue;
					}
					if (i == 2)
					{
						if (string.IsNullOrWhiteSpace(groupValue))
							coefficient = "1";
						else
							coefficient = groupValue;
					}
				}

				joinedCoefficient = String.Concat(sign, coefficient);
				coefficients.Add(joinedCoefficient);
				equationMatch = equationMatch.NextMatch();
				matchCount++;
			}
			return coefficients;
		}
	}
}
