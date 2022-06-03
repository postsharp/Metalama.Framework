// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Collections;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CompileTime
{
    // TODO: a class member should not store an ISymbol because we should not store references to a Roslyn compilation.

    internal record TemplateClassMember(
        string Name,
        TemplateClass TemplateClass,
        TemplateInfo TemplateInfo,
        SymbolId SymbolId,
        ImmutableArray<TemplateClassMemberParameter> Parameters,
        ImmutableArray<TemplateClassMemberParameter> TypeParameters,
        ImmutableDictionary<MethodKind, TemplateClassMember> Accessors )
    {
        public ImmutableDictionary<string, TemplateClassMemberParameter> IndexedParameters { get; } =
            Parameters.Concat( TypeParameters ).ToImmutableDictionary( x => x.Name, x => x );
    }

    internal record TemplateClassMemberParameter( int SourceIndex, string Name, bool IsCompileTime, int? TemplateIndex );
}