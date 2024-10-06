// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class RemoveAttributesTransformation : BaseSyntaxTreeTransformation, ITransformation
{
    public IRef<INamedType> AttributeType { get; }

    public RemoveAttributesTransformation(
        Advice advice,
        IRef<IDeclaration> targetDeclaration,
        IRef<INamedType> attributeType ) : base( advice )
    {
        this.AttributeType = attributeType;
        this.ContainingDeclaration = targetDeclaration;
    }

    public IRef<IDeclaration> ContainingDeclaration { get; }

    public override IRef<IDeclaration> TargetDeclaration => this.ContainingDeclaration;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.RemoveAttributes;

    public override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Remove attributes of type '{this.AttributeType}' from '{this.TargetDeclaration}'";
}