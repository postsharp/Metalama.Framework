// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class RemoveAttributesTransformation : BaseTransformation, ITransformation
{
    public INamedType AttributeType { get; }

    public RemoveAttributesTransformation(
        Advice advice,
        IDeclaration targetDeclaration,
        INamedType attributeType ) : base( advice )
    {
        this.AttributeType = attributeType;
        this.ContainingDeclaration = targetDeclaration;
    }

    public IDeclaration ContainingDeclaration { get; }

    public override IDeclaration TargetDeclaration => this.ContainingDeclaration;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override TransformationKind TransformationKind => TransformationKind.RemoveAttributes;

    public override FormattableString ToDisplayString() => $"Remove attributes of type '{this.AttributeType}' from '{this.TargetDeclaration}'";
}