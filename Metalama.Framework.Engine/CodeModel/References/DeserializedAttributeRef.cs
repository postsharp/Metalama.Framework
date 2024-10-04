// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class DeserializedAttributeRef : AttributeRef
{
    private readonly AttributeSerializationData _serializationData;

    public DeserializedAttributeRef( AttributeSerializationData serializationData, CompilationContext compilationContext ) : base(
        serializationData.ContainingDeclaration,
        serializationData.Type,
        compilationContext )
    {
        this._serializationData = serializationData;
    }

    public override bool TryGetTarget( CompilationModel compilation, IGenericContext? genericContext, [NotNullWhen( true )] out IAttribute? attribute )
    {
        attribute = compilation.Factory.GetDeserializedAttribute( this._serializationData );

        return true;
    }

    public override bool TryGetAttributeSerializationDataKey( [NotNullWhen( true )] out object? serializationDataKey )
    {
        serializationDataKey = this._serializationData;

        return true;
    }

    public override bool TryGetAttributeSerializationData( [NotNullWhen( true )] out AttributeSerializationData? serializationData )
    {
        serializationData = this._serializationData;

        return true;
    }

    public override string Name => throw new NotSupportedException();

    protected override AttributeSyntax? AttributeSyntax => null;

    protected override int GetHashCodeCore() => this._serializationData.GetHashCode();
}