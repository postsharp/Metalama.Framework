﻿using Caravela.Framework.Project;
using System;
using System.Diagnostics;
using System.Linq;

namespace Caravela.Framework.TestApp.Library
{
    [CompileTime]
    public static class ProcessInfo
    {
        public static string GetInfo() => $"{Process.GetCurrentProcess().ProcessName}, PID: {Process.GetCurrentProcess().Id}" + Environment.NewLine +
            string.Join( Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.IsDynamic && a.Location == "" ) );
    }
}
