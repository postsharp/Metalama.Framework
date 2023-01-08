// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class AddExplicitDefaultConstructorTransformation : BaseTransformation, ITypeLevelTransformation
{
    public AddExplicitDefaultConstructorTransformation( Advice advice, INamedType type ) : base( advice )
    {
        this.TargetType = type;
    }

    public INamedType TargetType { get; }

    public override IDeclaration TargetDeclaration => this.TargetType;

    public override TransformationObservability Observability => TransformationObservability.None;

    public override TransformationKind TransformationKind => TransformationKind.MakeDefaultConstructorExplicit;

    public override FormattableString ToDisplayString() => $"Add default constructor to '{this.TargetType}'";
}