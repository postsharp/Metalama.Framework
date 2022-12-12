// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

public static class RefExtensions
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    public static SyntaxTree? GetPrimarySyntaxTree( this IRef<IDeclaration> reference, Compilation compilation )
        => ((IRefImpl) reference).GetClosestSymbol( compilation ).GetPrimarySyntaxReference()?.SyntaxTree;
}