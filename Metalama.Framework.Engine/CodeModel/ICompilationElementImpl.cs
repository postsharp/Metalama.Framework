// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel;

internal interface ICompilationElementImpl : ICompilationElement
{
    new CompilationModel Compilation { get; }

    ICompilationElement? Translate(
        CompilationModel newCompilation,
        ReferenceResolutionOptions options = ReferenceResolutionOptions.Default,
        IGenericContext? genericContext = null );
}