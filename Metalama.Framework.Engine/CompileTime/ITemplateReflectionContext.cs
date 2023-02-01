// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime;

internal interface ITemplateReflectionContext
{
    ISymbolClassifier SymbolClassifier { get; }

    Compilation Compilation { get; }

    AttributeDeserializer AttributeDeserializer { get; }

    CompilationModel GetCompilationModel( ICompilation sourceCompilation );
}