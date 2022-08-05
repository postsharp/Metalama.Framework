﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal interface ICompilationVersion
{
    AssemblyIdentity AssemblyIdentity { get; }

    ulong CompileTimeProjectHash { get; }

    bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion );

}