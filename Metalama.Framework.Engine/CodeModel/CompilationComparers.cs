// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Comparers;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal class CompilationComparers : ICompilationComparers
{
    public CompilationComparers( ReflectionMapper reflectionMapper, Compilation compilation )
    {
        this.Default = new DeclarationEqualityComparer( reflectionMapper, compilation, false );
        this.WithNullability = new DeclarationEqualityComparer( reflectionMapper, compilation, true );
    }

    public IDeclarationComparer Default { get; }

    public ITypeComparer WithNullability { get; }
}