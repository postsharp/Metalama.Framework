// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal interface ISdkCompilation : ICompilation
{
    Compilation RoslynCompilation { get; }

    ISemanticModel GetCachedSemanticModel( SyntaxTree syntaxTree );

    ISdkDeclarationFactory Factory { get; }
}