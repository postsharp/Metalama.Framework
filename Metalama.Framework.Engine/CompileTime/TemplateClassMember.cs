// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CompileTime
{
    // TODO: a class member should not store an ISymbol because we should not store references to a Roslyn compilation.

    internal record TemplateClassMember(
        string Name,
        TemplateClass TemplateClass,
        TemplateInfo TemplateInfo,
        ISymbol Symbol,
        ImmutableArray<TemplateClassMemberParameter> Parameters,
        ImmutableArray<TemplateClassMemberParameter> TypeParameters );

    internal record TemplateClassMemberParameter( int Index, string Name, bool IsCompileTime );
}