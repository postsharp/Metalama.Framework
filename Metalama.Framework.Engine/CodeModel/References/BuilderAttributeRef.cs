// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Built;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// An implementation of <see cref="AttributeRef"/> based on <see cref="AttributeBuilder"/>.
/// </summary>
internal sealed class BuilderAttributeRef : AttributeRef
{
    public AttributeBuilderData AttributeBuilder { get; }

    public BuilderAttributeRef( AttributeBuilderData builder ) : base(
        builder.ContainingDeclaration,
        builder.Type )
    {
        this.AttributeBuilder = builder;
    }

    public override bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
    {
        attribute = new BuiltAttribute( this.AttributeBuilder, compilation, GenericContext.Empty );

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

    public override string Name => this.AttributeBuilder.Type.Name;

    protected override AttributeSyntax? AttributeSyntax => null;

    public override bool Equals( AttributeRef? other )
        => other is BuilderAttributeRef builderAttributeRef && this.AttributeBuilder.Equals( builderAttributeRef.AttributeBuilder );

    protected override int GetHashCodeCore() => this.AttributeBuilder.GetHashCode();
}