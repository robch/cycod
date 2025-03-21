using System;
using System.Collections.Generic;

public class TryCatchHelpers
{
    public static Action? NoThrowWrap(Action? action)
    {
        return action == null ? null : () =>
        {
            try { action(); }
            catch { }
        };
    }

    public static Action<T>? NoThrowWrap<T>(Action<T>? action)
    {
        return action == null ? null : (x =>
        {
            try { action(x); }
            catch { }
        });
    }

    public static Action<T1, T2>? NoThrowWrap<T1, T2>(Action<T1, T2>? action)
    {
        return action == null ? null : (x, y) =>
        {
            try { action(x, y); }
            catch { }
        };
    }

    public static Action<T1, T2, T3>? NoThrowWrap<T1, T2, T3>(Action<T1, T2, T3>? action)
    {
        return action == null ? null : (x, y, z) =>
        {
            try { action(x, y, z); }
            catch { }
        };
    }
}