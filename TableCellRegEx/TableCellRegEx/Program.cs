using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TableCellRegEx
{
	class Program
	{
		static void Main(string[] args)
		{
			string html = @"<table>
<tr>
<td>A simple nested table</td>
<td>
 <table>
 <tr><td>car</td></tr>
 <tr><td>bike</td></tr>
 </table>
</td>
</tr>
</table>";

			List<string> captures = new List<string>();

			var multilineTags = Regex.Matches(html, @"^</?td>\r*\n", RegexOptions.Multiline);
			if (multilineTags.Count == 2)
			{
				var from = multilineTags[0].Index + multilineTags[0].Length;
				var to = multilineTags[1].Index;
				captures.Add(html.Substring(from, to - from));
			}
			html = Regex.Replace(html, @"^</?td>\r*\n", "", RegexOptions.Multiline);

			var inlineTags = Regex.Matches(html, @"\<td\>(.*?)\</td\>", RegexOptions.Singleline);
			foreach (Match match in inlineTags)
				captures.Add(match.Groups[1].Value);

			foreach(var s in captures)
			{
				Console.WriteLine(s);
			}
		}
	}
}
