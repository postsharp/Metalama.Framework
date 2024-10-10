// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Built;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// An implementation of <see cref="AttributeRef"/> based on <see cref="BuilderData"/>.
/// </summary>
internal sealed class BuilderAttributeRef : AttributeRef
{
    public AttributeBuilderData BuilderData { get; }

    public BuilderAttributeRef( AttributeBuilderData builderData )
    {
        this.BuilderData = builderData;
    }

    public override IRef<IDeclaration> ContainingDeclaration => this.BuilderData.ContainingDeclaration;

    public override IRef<INamedType> AttributeType => this.BuilderData.Constructor.DeclaringType.AssertNotNull();

    public override bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
    {
        attribute = new BuiltAttribute( this.BuilderData, compilation, GenericContext.Empty );

        return true;
    }

    public override bool TryGetAttributeSerializationDataKey( [NotNullWhen( true )] out object? serializationDataKey )
    {
        serializationDataKey = this.BuilderData;

        return true;
    }

    public override bool TryGetAttributeSerializationData( [NotNullWhen( true )] out AttributeSerializationData? serializationData )
    {
        serializationData = new AttributeSerializationData( this.BuilderData );

        return true;
    }

    public override string Name => this.BuilderData.Type.Name.AssertNotNull();

    protected override AttributeSyntax? AttributeSyntax => null;

    public override bool Equals( AttributeRef? other )
        => other is BuilderAttributeRef builderAttributeRef && this.BuilderData.Equals( builderAttributeRef.BuilderData );

    protected override int GetHashCodeCore() => this.BuilderData.GetHashCode();
}