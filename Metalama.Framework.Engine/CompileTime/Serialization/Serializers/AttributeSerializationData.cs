// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Serialization;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime.Serialization.Serializers;

internal class AttributeSerializationData
{
    public Ref<IDeclaration> ContainingDeclaration { get; }

    public Ref<INamedType> Type { get; }

    public Ref<IConstructor> Constructor { get; }

    public ImmutableArray<TypedConstantRef> ConstructorArguments { get; }

    public ImmutableArray<KeyValuePair<string, TypedConstantRef>> NamedArguments { get; }

    public AttributeSerializationData( ISymbol symbol, AttributeData attributeData, CompilationContext compilationContext )
    {
        this.ContainingDeclaration = Ref.FromSymbol<IDeclaration>( symbol, compilationContext );
        this.Type = Ref.FromSymbol<INamedType>( attributeData.AttributeClass.AssertSymbolNotNull(), compilationContext );
        this.Constructor = Ref.FromSymbol<IConstructor>( attributeData.AttributeConstructor.AssertSymbolNotNull(), compilationContext );
        this.ConstructorArguments = attributeData.ConstructorArguments.SelectAsImmutableArray( c => c.ToOurTypedConstantRef( compilationContext ) );

        this.NamedArguments =
            attributeData.NamedArguments.SelectAsImmutableArray(
                x => new KeyValuePair<string, TypedConstantRef>( x.Key, x.Value.ToOurTypedConstantRef( compilationContext ) ) );
    }

    public AttributeSerializationData( AttributeBuilder builder )
    {
        this.ContainingDeclaration = builder.ContainingDeclaration.ToValueTypedRef();
        this.Constructor = builder.Constructor.ToValueTypedRef();
        this.Type = builder.Type.ToValueTypedRef();
        this.ConstructorArguments = builder.ConstructorArguments.SelectAsImmutableArray( a => a.ToRef() );
        this.NamedArguments = builder.NamedArguments.SelectAsImmutableArray( x => new KeyValuePair<string, TypedConstantRef>( x.Key, x.Value.ToRef() ) );
    }

    public AttributeSerializationData( IArgumentsReader reader )
    {
        this.ContainingDeclaration = reader.GetValue<Ref<IDeclaration>>( nameof(this.ContainingDeclaration) );
        this.Type = reader.GetValue<Ref<INamedType>>( nameof(this.Type) );
        this.Constructor = reader.GetValue<Ref<IConstructor>>( nameof(this.Constructor) );
        this.ConstructorArguments = reader.GetValue<ImmutableArray<TypedConstantRef>>( nameof(this.ConstructorArguments) );

        if ( this.ConstructorArguments.IsDefault )
        {
            this.ConstructorArguments = ImmutableArray<TypedConstantRef>.Empty;
        }

        var namedArgumentKeys = reader.GetValue<string[]>( "NamedArgumentKeys" );
        var namedArgumentValues = reader.GetValue<TypedConstantRef[]>( "NamedArgumentValues" );

        if ( namedArgumentKeys != null && namedArgumentValues != null )
        {
            var namedArguments = ImmutableArray.CreateBuilder<KeyValuePair<string, TypedConstantRef>>( namedArgumentKeys.Length );

            for ( var i = 0; i < namedArgumentKeys.Length; i++ )
            {
                namedArguments.Add( new KeyValuePair<string, TypedConstantRef>( namedArgumentKeys[i], namedArgumentValues[i] ) );
            }

            this.NamedArguments = namedArguments.MoveToImmutable();
        }
        else
        {
            this.NamedArguments = ImmutableArray<KeyValuePair<string, TypedConstantRef>>.Empty;
        }
    }

    public void Serialize( IArgumentsWriter writer )
    {
        writer.SetValue( nameof(this.ContainingDeclaration), this.ContainingDeclaration );
        writer.SetValue( nameof(this.Constructor), this.Constructor );
        writer.SetValue( nameof(this.Type), this.Type );

        if ( this.ConstructorArguments.Length > 0 )
        {
            writer.SetValue( nameof(this.ConstructorArguments), this.ConstructorArguments );
        }

        if ( this.NamedArguments.Length > 0 )
        {
            writer.SetValue( "NamedArgumentKeys", this.NamedArguments.SelectAsArray( n => n.Key ) );
            writer.SetValue( "NamedArgumentValues", this.NamedArguments.SelectAsArray( n => n.Value ) );
        }
    }
}