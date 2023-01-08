// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Comparers;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class CompilationComparers : ICompilationComparers
{
    public CompilationComparers( ReflectionMapper reflectionMapper, Compilation compilation )
    {
        this.Default = new DeclarationEqualityComparer( reflectionMapper, compilation, false );
        this.IncludeNullability = new DeclarationEqualityComparer( reflectionMapper, compilation, true );
    }

    public IDeclarationComparer Default { get; }

    public ITypeComparer IncludeNullability { get; }

    public ITypeComparer GetTypeComparer( TypeComparison comparison )
        => comparison switch
        {
            TypeComparison.Default => this.Default,
            TypeComparison.IncludeNullability => this.IncludeNullability,
            _ => throw new ArgumentOutOfRangeException()
        };
}