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

			if (timeSpan.TotalDays >= 365)
			{
				int years = (int)(timeSpan.TotalDays / 365);
				return $"Прошло {years} {GetPluralForm(years, "год", "года", "лет")}";
			}
			else if (timeSpan.TotalDays >= 30)
			{
				int months = (int)(timeSpan.TotalDays / 30);
				return $"Прошло {months} {GetPluralForm(months, "месяц", "месяца", "месяцев")}";
			}
			else if (timeSpan.TotalDays >= 7)
			{
				int weeks = (int)(timeSpan.TotalDays / 7);
				return $"Прошло {weeks} {GetPluralForm(weeks, "неделя", "недели", "недель")}";
			}
			else if (timeSpan.TotalDays >= 1)
			{
				int days = (int)timeSpan.TotalDays;
				return $"Прошло {days} {GetPluralForm(days, "день", "дня", "дней")}";
			}
			else if (timeSpan.TotalHours >= 1)
			{
				int hours = (int)timeSpan.TotalHours;
				return $"Прошло {hours} {GetPluralForm(hours, "час", "часа", "часов")}";
			}
			else if (timeSpan.TotalMinutes >= 1)
			{
				int minutes = (int)timeSpan.TotalMinutes;
				return $"Прошло {minutes} {GetPluralForm(minutes, "минута", "минуты", "минут")}";
			}
			else
			{
				int seconds = (int)timeSpan.TotalSeconds;
				return $"Прошло {seconds} {GetPluralForm(seconds, "секунда", "секунды", "секунд")}";
			}
		}

		private static string GetPluralForm(int value, string singular, string few, string many)
		{
			if (value % 10 == 1 && value % 100 != 11)
			{
				return singular;
			}
			else if (value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20))
			{
				return few;
			}
			else
			{
				return many;
			}
		}
	}
}
