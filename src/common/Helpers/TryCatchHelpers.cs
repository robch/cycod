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

    public static Action<T1, T2, T3, T4>? NoThrowWrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4>? action)
    {
        return action == null ? null : (x, y, z, a) =>
        {
            try { action(x, y, z, a); }
            catch { }
        };
    }

    public static Func<(T? value, Exception? ex)> NoThrowWrapException<T>(Func<T> function, T defaultResult)
    {
        return () =>
        {
            try
            {
                return (function(), null);
            }
            catch (Exception ex)
            {
                return (defaultResult, ex);
            }
        };
    }

    public static Func<T1, (T2? value, Exception? ex)> NoThrowWrapException<T1, T2>(Func<T1, T2> function, T2 defaultResult)
    {
        return (x) =>
        {
            try
            {
                return (function(x), null);
            }
            catch (Exception ex)
            {
                return (defaultResult, ex);
            }
        };
    }

    public static Func<T1, T2, (T3? value, Exception? ex)> NoThrowWrapException<T1, T2, T3>(Func<T1, T2, T3> function, T3 defaultResult)
    {
        return (x, y) =>
        {
            try
            {
                return (function(x, y), null);
            }
            catch (Exception ex)
            {
                return (defaultResult, ex);
            }
        };
    }

    public static Func<T1, T2, T3, (T4? value, Exception? ex)> NoThrowWrapException<T1, T2, T3, T4>(Func<T1, T2, T3, T4> function, T4 defaultResult)
    {
        return (x, y, z) =>
        {
            try
            {
                return (function(x, y, z), null);
            }
            catch (Exception ex)
            {
                return (defaultResult, ex);
            }
        };
    }

    public static Func<T1, T2, T3, T4, (T5? value, Exception? ex)> NoThrowWrapException<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> function, T5 defaultResult)
    {
        return (x, y, z, a) =>
        {
            try
            {
                return (function(x, y, z, a), null);
            }
            catch (Exception ex)
            {
                return (defaultResult, ex);
            }
        };
    }

    public static T TryCatchNoThrow<T>(Func<T> function, T defaultResult, out Exception? functionThrewException)
    {
        functionThrewException = null;
        try
        {
            return function();
        }
        catch (Exception ex)
        {
            functionThrewException = ex;
        }
        return defaultResult;
    }
}