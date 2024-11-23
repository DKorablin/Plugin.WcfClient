using System;
using System.Collections.Generic;

namespace Plugin.WcfClient.Extensions
{
	internal static class ListExtension
	{
		public static T Find<T>(this IList<T> list, Predicate<T> match)
		{
			if(match == null)
				throw new ArgumentNullException("match");

			for(int i = 0; i < list.Count; i++)
			{
				if(match(list[i]))
					return list[i];
			}
			return default(T);
		}
	}
}