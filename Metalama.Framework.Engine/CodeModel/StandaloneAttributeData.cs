// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class StandaloneAttributeData : IAttributeData
{
    public StandaloneAttributeData( IConstructor constructor )
    {
        this.Constructor = constructor;
    }

    public INamedType Type => this.Constructor.DeclaringType;

    public IConstructor Constructor { get; }

    public ImmutableArray<TypedConstant> ConstructorArguments { get; init; } = ImmutableArray<TypedConstant>.Empty;

    public INamedArgumentList NamedArguments { get; } = NamedArgumentList.Empty;

    bool IAttributeData.TryConstruct( ScopedDiagnosticSink diagnosticSink, [NotNullWhen( true )] out System.Attribute? attribute )
        => throw new NotImplementedException();
}