//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.ComponentModel;

public class DateAndTimeHelperFunctions
{
    [Description("Gets the current date.")]
    public string GetCurrentDate()
    {
        var date = DateTime.Now;
        return $"{date.Year}-{date.Month}-{date.Day}";
    }

    [Description("Gets the current time.")]
    public string GetCurrentTime()
    {
        var date = DateTime.Now;
        return $"{date.Hour}:{date.Minute}:{date.Second}";
    }
}