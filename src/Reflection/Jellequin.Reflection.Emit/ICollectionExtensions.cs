using System.Collections.Generic;

namespace Jellequin.Reflection.Emit
{
	static class ICollectionExtensions
	{
		internal static TValue AddWithReturn<TKey, TValue>(this IDictionary<TKey,TValue> dict,TKey key,TValue value)
		{
			dict.Add(key,value);
			return value;
		}

		internal static T AddWithReturn<T>(this ICollection<T> coll,T value)
		{
			coll.Add(value);
			return value;
		}
	}
}
