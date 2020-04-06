using System;

namespace FlexAS
{
	public class DateUtility
	{
		public static string GetFormattedTimeFromSeconds(uint seconds)
		{
			TimeSpan t = TimeSpan.FromSeconds(seconds);

			if (t.Minutes <= 0)
				return string.Format("{0:D2}s", t.Seconds);
			else if (t.Hours <= 0)
				return string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);
			else if (t.Days <= 0)
				return string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
			else
				return string.Format("{0:D1}d:{1:D2}h:{2:D2}m:{3:D2}s", t.Days, t.Hours, t.Minutes, t.Seconds);
		}
	}
}
