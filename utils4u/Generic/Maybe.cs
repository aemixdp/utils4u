using System;
using System.Collections.Generic;

public struct Maybe<T>
{
    private readonly T value;
    public readonly bool HasValue;

    public Maybe(T value, bool hasValue)
    {
        this.value = value;
        this.HasValue = hasValue;
    }

    public Maybe<X> Map<X>(Func<T, X> fn)
    {
        return HasValue
            ? new Maybe<X>(fn(value), true)
            : new Maybe<X>(default(X), false);
    }

    public T GetOrElse(T defaultValue)
    {
        return HasValue ? value : defaultValue;
    }

    public static Maybe<X> Wrap<X>(X value) where X : class
    {
        return value == null
            ? new Maybe<X>(null, false)
            : new Maybe<X>(value, true);
    }
}

public static class Maybe
{
    public static Maybe<T> Some<T>(T value)
    {
        return new Maybe<T>(value, true);
    }

    public static Maybe<T> None<T>()
    {
        return new Maybe<T>(default(T), false);
    }

    public static Maybe<V> Lookup<K, V>(this Dictionary<K, V> dictionary, K key)
    {
        V val;
        return dictionary.TryGetValue(key, out val)
            ? Maybe.Some(val)
            : Maybe.None<V>();
    }
}
