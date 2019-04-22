﻿namespace Cappta.Logging.Extensions
{
	internal static class StringExtensions
	{
		public static string ToCamelCase(this string value)
		{
			if(char.IsLower(value[0])) { return value; }

			return char.ToLower(value[0]) + value.Substring(1);
		}

		public static string ToTitleCase(this string value)
		{
			if (char.IsUpper(value[0])) { return value; }

			return char.ToUpper(value[0]) + value.Substring(1);
		}
	}
}
