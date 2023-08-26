// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal interface ISdkCompilation : ICompilation
{
    Compilation RoslynCompilation { get; }

    SemanticModel GetCachedSemanticModel( SyntaxTree syntaxTree );

    ISdkDeclarationFactory Factory { get; }
}