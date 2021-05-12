// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.Linq;
using Caravela.Framework.Project;

namespace Caravela.Framework.TestApp.Library
{
    [CompileTime]
    public static class ProcessInfo
    {
        public static string GetInfo() => $"{Process.GetCurrentProcess().ProcessName}, PID: {Process.GetCurrentProcess().Id}" + Environment.NewLine +
            string.Join( Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.IsDynamic && a.Location == "" ) );
    }
}
