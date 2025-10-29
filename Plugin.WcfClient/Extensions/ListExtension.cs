using System;
using System.Collections.Generic;

namespace Plugin.WcfClient.Extensions
{
	internal static class ListExtension
	{
		public static T Find<T>(this IList<T> list, Predicate<T> match)
		{
			_ = match ?? throw new ArgumentNullException(nameof(match));

			for(Int32 i = 0; i < list.Count; i++)
			{
				if(match(list[i]))
					return list[i];
			}
			return default;
		}
	}
}