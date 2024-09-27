// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface ICompilationBoundRefImpl : IRefImpl
{
    CompilationContext CompilationContext { get; }

    ResolvedAttributeRef GetAttributeData();

    bool IsDefinition { get; }

    IRef Definition { get; }

    ICompilationBoundRefImpl WithGenericContext( GenericContext genericContext );

    IRefStrategy Strategy { get; }
}