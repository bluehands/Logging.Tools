using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bluehands.Repository.Diagnostics;

public class ScriptRunnerFormatProvider : LogFormatProviderBase
{
	public override bool IsNewLogLine(string line)
	{
		const int minLength = 23;
		if (string.IsNullOrEmpty(line) || line.Length < minLength)
		{
			return false;
		}
		return DateTime.TryParseExact(line.Substring(0, minLength), "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
	}


	static readonly Regex s_LineRegex = new(@"(?'date'\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}) \[(?'threaId'\d+|\w*\s#\d+)\s*:(?'logger'[^:]*):(?'level'[^(\s|\])]*)(\s|\])*(?'message'.*)",
		RegexOptions.Compiled);

	public override LogListViewItem Convert(string line, out bool succeeded)
	{
		var match = s_LineRegex.Match(line);
		succeeded = match.Success;
		var item = new LogListViewItem();
		if (succeeded)
		{
			item.Instance = match.Groups["logger"].Value;
			item.Level = match.Groups["level"].Value;
			item.Message = match.Groups["message"].Value;
			item.ThreadId = match.Groups["threaId"].Value;
			item.Time = match.Groups["date"].Value;
		}

		return item;
	}

	public override bool IsTraceEnter(LogListViewItem item) => item.Message.Contains(">>>Start>>");

	public override bool IsTraceLeave(LogListViewItem item) => item.Message.Contains("<<<Done<<<");

	//2022-05-11 08:53:34,109 [7 :Util:INFO ] EventProcessor`1.Process: <<<Done<<< Applying event: ConnectorInstancePatched 7632243c-944c-46dd-902e-133670647ded: 00:00:00.0009121
	protected override double GetTimeFromTraceLeave(string message)
	{
		double result;
		try
		{
			var parts = message.Split(':');
			if (parts.Length < 4)
				return -1;

			var input = $"{parts[parts.Length - 3]}:{parts[parts.Length - 2]}:{parts[parts.Length - 1]}";
			var timeSpan = TimeSpan.Parse(input);
			return timeSpan.TotalMilliseconds;
		}
		catch (Exception)
		{
			result = -1;
		}
		return result;
	}
}