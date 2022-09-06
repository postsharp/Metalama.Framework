// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Diagnostics;

namespace Metalama.Framework.CompilerExtensions;

public static class ProcessKindHelper
{
    public static ProcessKind CurrentProcessKind { get; } =
        Process.GetCurrentProcess().ProcessName.ToLowerInvariant() switch
        {
            "devenv" => ProcessKind.DevEnv,
            "servicehub.roslyncodeanalysisservice" => ProcessKind.RoslynCodeAnalysisService,
            "csc" => ProcessKind.Compiler,
            "dotnet" =>
                Environment.CommandLine.Contains( "JetBrains.ReSharper.Roslyn.Worker.exe" ) ? ProcessKind.Rider :
                Environment.CommandLine.Contains( "VBCSCompiler.dll" ) ? ProcessKind.Compiler :
                ProcessKind.Other,
            _ => ProcessKind.Other
        };
}

public enum ProcessKind
{
    Other,
    Compiler,
    DevEnv,
    RoslynCodeAnalysisService,
    Rider
}