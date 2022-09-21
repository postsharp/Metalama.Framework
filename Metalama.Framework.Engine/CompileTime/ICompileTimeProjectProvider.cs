﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime;

public interface ICompileTimeProjectProvider
{
    bool TryGetForCompilation( Compilation compilation, out CompileTimeProject compileTimeProject );
}