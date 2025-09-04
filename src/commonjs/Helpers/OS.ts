//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

export class OS {
    public static IsWindows(): boolean {
        return process.platform === 'win32';
    }
    
    public static IsMac(): boolean {
        return process.platform === 'darwin';
    }
    
    public static IsLinux(): boolean {
        return process.platform === 'linux' && !OS.IsAndroid();
    }
    
    public static IsAndroid(): boolean {
        return process.env.ANDROID_ROOT != null && process.env.ANDROID_DATA != null;
    }
    
    public static IsCodeSpaces(): boolean {
        return process.env.CODESPACES === 'true';
    }
}