using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
	public static class ElapsedTimeFormatter
	{
		public static string FormatTimeAgo(DateTime pastDate)
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now - pastDate;

			//if (timeSpan.TotalDays >= 365)
			//{
			//	int years = (int)(timeSpan.TotalDays / 365);
			//	return $"{years} г.";
			//}

			if (timeSpan.TotalDays >= 30)
			{
				int months = (int)(timeSpan.TotalDays / 30);
				return $"{months} мес.";
			}
			else if (timeSpan.TotalDays >= 7)
			{
				int weeks = (int)(timeSpan.TotalDays / 7);
				return $"{weeks} нед.";
			}
			else if (timeSpan.TotalDays >= 1)
			{
				int days = (int)timeSpan.TotalDays;
				return $"{days} дн.";
			}
			else if (timeSpan.TotalHours >= 1)
			{
				int hours = (int)timeSpan.TotalHours;
				return $"{hours} час.";
			}
			else if (timeSpan.TotalMinutes >= 1)
			{
				int minutes = (int)timeSpan.TotalMinutes;
				return $"{minutes} мин.";
			}
			else
			{
				int seconds = (int)timeSpan.TotalSeconds;
				return $"{seconds} сек.";
			}
		}
	}
}
