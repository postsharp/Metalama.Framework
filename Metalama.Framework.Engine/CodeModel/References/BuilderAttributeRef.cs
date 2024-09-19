// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class BuilderAttributeRef : AttributeRef
{
    public AttributeBuilder AttributeBuilder { get; }

    public BuilderAttributeRef( AttributeBuilder builder ) : base(
        builder.ContainingDeclaration.ToRef(),
        builder.Constructor.DeclaringType.ToRef(),
        builder.GetCompilationContext() )
    {
        this.AttributeBuilder = builder;
    }

    public override bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
    {
        attribute = new BuiltAttribute( this.AttributeBuilder, compilation, genericContext ?? NullGenericContext.Instance );

        return true;
    }

    public override bool TryGetAttributeSerializationDataKey( [NotNullWhen( true )] out object? serializationDataKey )
    {
        serializationDataKey = this.AttributeBuilder;

        return true;
    }

    public override bool TryGetAttributeSerializationData( [NotNullWhen( true )] out AttributeSerializationData? serializationData )
    {
        serializationData = new AttributeSerializationData( this.AttributeBuilder );

        return true;
    }

    protected override AttributeSyntax? AttributeSyntax => null;

    protected override int GetHashCodeCore() => this.AttributeBuilder.GetHashCode();
}