using System.Collections.Generic;

public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    /// <summary>
    /// Gets a random element from IList or default if list is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ts"></param>
    /// <returns></returns>
    public static T GetRandomItem<T>(this IList<T> ts)
    {
        if (ts.Count == 0)
        {
            return default(T);
        }
        return ts[UnityEngine.Random.Range(0, ts.Count)];
    }
}