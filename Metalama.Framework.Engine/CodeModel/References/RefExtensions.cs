// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

public static class RefExtensions
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    public static SyntaxTree? GetPrimarySyntaxTree( this IRef<IDeclaration> reference, CompilationContext compilationContext )
        => ((IRefImpl) reference).GetClosestSymbol( compilationContext ).GetPrimarySyntaxReference()?.SyntaxTree;
}