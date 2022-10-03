﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.Linq;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.TestApp.Library
{
    [CompileTime]
    public static class ProcessInfo
    {
        // TODO #31038: Remove the parentheses from the interpolated string. This is here as a workaround to this issue.
        public static string GetInfo() => $"{(Process.GetCurrentProcess().ProcessName)}, PID: {(Process.GetCurrentProcess().Id)}" + Environment.NewLine +
            string.Join( Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.IsDynamic && a.Location == "" ) );
    }
}
