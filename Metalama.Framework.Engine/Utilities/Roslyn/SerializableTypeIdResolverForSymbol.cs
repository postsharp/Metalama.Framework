// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public class SerializableTypeIdResolverForSymbol : SerializableTypeIdResolver<ITypeSymbol, INamespaceOrTypeSymbol>
{
    private readonly Compilation _compilation;

    internal SerializableTypeIdResolverForSymbol( Compilation compilation )
    {
#if DEBUG
        if ( compilation.AssemblyName == "empty" )
        {
            throw new AssertionFailedException( "Expected a non-empty assembly." );
        }
#endif
        this._compilation = compilation;
    }

    public ITypeSymbol ResolveId( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? genericArguments = null )
    {
        var genericArgumentSymbols = genericArguments?.ToDictionary( kv => kv.Key, kv => kv.Value.GetSymbol() );

        return this.ResolveId( typeId, genericArgumentSymbols! );
    }

    protected override ITypeSymbol CreateArrayType( ITypeSymbol elementType, int rank )
        => this._compilation.CreateArrayTypeSymbol( elementType, rank );

    protected override ITypeSymbol CreatePointerType( ITypeSymbol pointedAtType )
        => this._compilation.CreatePointerTypeSymbol( pointedAtType );

    protected override ITypeSymbol CreateNullableType( ITypeSymbol elementType )
        => elementType.IsValueType
            ? this._compilation.GetSpecialType( SpecialType.System_Nullable_T ).Construct( elementType )
            : elementType.WithNullableAnnotation( NullableAnnotation.Annotated );

    protected override ITypeSymbol CreateNonNullableReferenceType( ITypeSymbol referenceType )
        => referenceType.WithNullableAnnotation( NullableAnnotation.NotAnnotated );

    protected override ITypeSymbol ConstructGenericType( ITypeSymbol genericType, ITypeSymbol[] typeArguments )
        => genericType.AssertCast<INamedTypeSymbol>().Construct( typeArguments );

    protected override ITypeSymbol CreateTupleType( ImmutableArray<ITypeSymbol> elementTypes )
        => this._compilation.CreateTupleTypeSymbol( elementTypes );

    protected override ITypeSymbol DynamicType => this._compilation.DynamicType;

    protected override INamespaceOrTypeSymbol? LookupName( string name, int arity, INamespaceOrTypeSymbol? ns )
    {
        ns ??= this._compilation.GlobalNamespace;

        var candidates = ns.GetMembers( name );

        foreach ( var member in candidates )
        {
            var memberArity = member.Kind == SymbolKind.Namespace ? 0 : ((INamedTypeSymbol) member).Arity;

            if ( arity == memberArity )
            {
                return (INamespaceOrTypeSymbol) member;
            }
        }

        return null;
    }

    protected override ITypeSymbol GetSpecialType( SpecialType specialType ) => this._compilation.GetSpecialType( specialType );

    protected override bool HasTypeParameterOfName( ITypeSymbol type, string name )
        => type.AssertCast<INamedTypeSymbol>().TypeParameters.Any( t => t.Name == name );
}