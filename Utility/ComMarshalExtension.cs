using System;

namespace Hi3Helper.Plugin.Core.Utility;

public static class ComMarshalExtension
{
    public static T[] CreateArrayFromSelector<T>(Func<int> countCallback, Func<int, T> selectorCallback)
    {
        int count = countCallback();
        if (count == 0)
        {
            return [];
        }

        T[] values = new T[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = selectorCallback(i);
        }

        return values;
    }
}
