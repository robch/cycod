using System;
using System.Collections.Generic;

public class TryCatchHelpers
{
    public static Action<T>? NoThrowWrap<T>(Action<T>? action)
    {
        return action == null ? null : (x =>
        {
            try { action(x); }
            catch { }
        });
    }
}