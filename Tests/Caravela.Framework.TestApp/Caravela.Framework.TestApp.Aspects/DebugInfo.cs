// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using Caravela.Framework.TestApp.Library;

namespace Caravela.Framework.TestApp.Aspects
{
    [CompileTime]
    public static class DebugInfo
    {
        public static string GetInfo() => ProcessInfo.GetInfo();
    }
}
