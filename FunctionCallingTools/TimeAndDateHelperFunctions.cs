//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System;

public class TimeAndDateHelperFunctions
{
    [HelperFunctionDescription("Gets the current date.")]
    public static string GetCurrentDate()
    {
        var date = DateTime.Now;
        return $"{date.Year}-{date.Month}-{date.Day}";
    }

    [HelperFunctionDescription("Gets the current time.")]
    public static string GetCurrentTime()
    {
        var date = DateTime.Now;
        return $"{date.Hour}:{date.Minute}:{date.Second}";
    }
}