// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static class SerializableDeclarationIdProvider
{
    private static readonly char[] _separators = new[] { ';', '=' };

    public static SerializableDeclarationId GetSerializableId( this ISymbol symbol ) => symbol.GetSerializableId( DeclarationRefTargetKind.Default );

    internal static SerializableDeclarationId GetSerializableId( this ISymbol symbol, DeclarationRefTargetKind targetKind )
    {
        switch ( symbol )
        {
            case IParameterSymbol parameterSymbol:
                {
                    var parentId = DocumentationCommentId.CreateDeclarationId( parameterSymbol.ContainingSymbol ).AssertNotNull();

                    return new SerializableDeclarationId( $"{parentId};Parameter={parameterSymbol.Ordinal}" );
                }

            case ITypeParameterSymbol typeParameterSymbol:
                {
                    var parentId = DocumentationCommentId.CreateDeclarationId( typeParameterSymbol.ContainingSymbol ).AssertNotNull();

                    return new SerializableDeclarationId( $"{parentId};TypeParameter={typeParameterSymbol.Ordinal}" );
                }
        }

        var id = DocumentationCommentId.CreateDeclarationId( symbol );

        if ( id == null )
        {
            throw new ArgumentOutOfRangeException( $"Cannot create a {nameof(SerializableDeclarationId)} for '{symbol}'." );
        }

        if ( targetKind == DeclarationRefTargetKind.Default )
        {
            return new SerializableDeclarationId( id );
        }
        else
        {
            return new SerializableDeclarationId( $"{id};{targetKind}" );
        }
    }

    public static bool TryGetSerializableId( this ISymbol? symbol, out SerializableDeclarationId id )
    {
        switch ( symbol )
        {
            case null:
                id = default;

                return false;

            case IParameterSymbol parameterSymbol:
                {
                    var parentId = DocumentationCommentId.CreateDeclarationId( parameterSymbol.ContainingSymbol ).AssertNotNull();
                    id = new SerializableDeclarationId( $"{parentId}@{parameterSymbol.Ordinal}" );

                    break;
                }

            default:
                {
                    var str = DocumentationCommentId.CreateDeclarationId( symbol );
                    id = new SerializableDeclarationId( str );

                    break;
                }
        }

        return true;
    }

    public static ISymbol? ResolveToSymbol( this SerializableDeclarationId id, Compilation compilation )
    {
        var indexOfAt = id.Id.IndexOfOrdinal( ';' );

        if ( indexOfAt > 0 )
        {
            // We have a parameter or a type parameter.

            var parts = id.Id.Split( _separators );

            var parentId = parts[0];
            var kind = parts[1];
            var ordinal = parts.Length == 3 ? int.Parse( parts[2], CultureInfo.InvariantCulture ) : -1;

            var parent = DocumentationCommentId.GetFirstSymbolForDeclarationId( parentId, compilation );

            return (parent, kind) switch
            {
                (null, _) => null,
                (IMethodSymbol method, "Parameter") => method.Parameters[ordinal],
                (IMethodSymbol method, "TypeParameter") => method.TypeParameters[ordinal],
                (INamedTypeSymbol type, "TypeParameter") => type.TypeParameters[ordinal],
                (IPropertySymbol property, "Parameter") => property.Parameters[ordinal],
                _ => null
            };
        }
        else
        {
            return DocumentationCommentId.GetFirstSymbolForDeclarationId( id.ToString(), compilation );
        }
    }

    public static IDeclaration? ResolveToDeclaration( this SerializableDeclarationId id, CompilationModel compilation )
    {
        var indexOfAt = id.Id.IndexOfOrdinal( ';' );

        if ( indexOfAt > 0 )
        {
            // We have a parameter or a type parameter.

            var parts = id.Id.Split( _separators );

            var parentId = parts[0];
            var kind = parts[1];
            var ordinal = parts.Length == 3 ? int.Parse( parts[2], CultureInfo.InvariantCulture ) : -1;

            var parent = DocumentationCommentId.GetFirstSymbolForDeclarationId( parentId, compilation.RoslynCompilation );

            return (parent, kind) switch
            {
                (null, _) => null,
                (IMethodSymbol method, "Parameter") => compilation.Factory.GetParameter( method.Parameters[ordinal] ),
                (IPropertySymbol property, "Parameter") => compilation.Factory.GetIndexer( property ).Parameters[ordinal],
                (IMethodSymbol method, "TypeParameter") => compilation.Factory.GetMethod( method ).TypeParameters[ordinal],
                (INamedTypeSymbol type, "TypeParameter") => compilation.Factory.GetNamedType( type ).TypeParameters[ordinal],
                (IMethodSymbol method, nameof(DeclarationRefTargetKind.Return)) => compilation.Factory.GetMethod( method ).ReturnParameter,
                (IFieldSymbol field, nameof(DeclarationRefTargetKind.PropertyGet)) => compilation.Factory.GetField( field ).GetMethod,
                (IFieldSymbol field, nameof(DeclarationRefTargetKind.PropertySet)) => compilation.Factory.GetField( field ).SetMethod,
                (IFieldSymbol field, nameof(DeclarationRefTargetKind.PropertySetParameter)) => compilation.Factory.GetField( field ).SetMethod?.Parameters[0],
                (IFieldSymbol field, nameof(DeclarationRefTargetKind.PropertyGetReturnParameter)) => compilation.Factory.GetField( field )
                    .GetMethod?.ReturnParameter,
                (IFieldSymbol field, nameof(DeclarationRefTargetKind.PropertySetReturnParameter)) => compilation.Factory.GetField( field )
                    .SetMethod?.ReturnParameter,
                (IEventSymbol eventSymbol, nameof(DeclarationRefTargetKind.EventRaise)) => compilation.Factory.GetEvent( eventSymbol ).RaiseMethod,
                (IEventSymbol eventSymbol, nameof(DeclarationRefTargetKind.EventRaiseParameter)) => compilation.Factory.GetEvent( eventSymbol )
                    .RaiseMethod?.Parameters[0],
                (IEventSymbol eventSymbol, nameof(DeclarationRefTargetKind.EventRaiseReturnParameter)) => compilation.Factory.GetEvent( eventSymbol )
                    .RaiseMethod?.ReturnParameter,
                _ => null
            };
        }
        else
        {
            var symbol = DocumentationCommentId.GetFirstSymbolForDeclarationId( id.ToString(), compilation.RoslynCompilation );

            if ( symbol == null )
            {
                return null;
            }

            return compilation.Factory.GetDeclaration( symbol );
        }
    }
}