// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static class SerializableDeclarationIdProvider
{
    private static readonly char[] _separators = new[] { ';', '=' };
    private const string _assemblyPrefix = "Assembly:";

    public static SerializableDeclarationId GetSerializableId( this ISymbol symbol ) => symbol.GetSerializableId( DeclarationRefTargetKind.Default );

    internal static SerializableDeclarationId GetSerializableId( this ISymbol symbol, DeclarationRefTargetKind targetKind )
    {
        if ( !TryGetSerializableId( symbol, targetKind, out var id ) )
        {
            throw new ArgumentOutOfRangeException( $"Cannot create a SerializableDeclarationId for '{symbol}'." );
        }

        return id;
    }

    public static bool TryGetSerializableId( this ISymbol? symbol, out SerializableDeclarationId id )
        => TryGetSerializableId( symbol, DeclarationRefTargetKind.Default, out id );

    internal static bool TryGetSerializableId( this ISymbol? symbol, DeclarationRefTargetKind targetKind, out SerializableDeclarationId id )
    {
        switch ( symbol )
        {
            case null:
            case ILocalSymbol:
            case IMethodSymbol
            {
                MethodKind: MethodKind.LocalFunction or MethodKind.AnonymousFunction or MethodKind.DelegateInvoke
            }:

                id = default;

                return false;

            case IParameterSymbol parameterSymbol:
                {
                    var parentId = DocumentationCommentId.CreateDeclarationId( parameterSymbol.ContainingSymbol ).AssertNotNull();

                    id = new SerializableDeclarationId( $"{parentId};Parameter={parameterSymbol.Ordinal}" );

                    return true;
                }

            case ITypeParameterSymbol typeParameterSymbol:
                {
                    var parentId = DocumentationCommentId.CreateDeclarationId( typeParameterSymbol.ContainingSymbol ).AssertNotNull();

                    id = new SerializableDeclarationId( $"{parentId};TypeParameter={typeParameterSymbol.Ordinal}" );

                    return true;
                }

            case IAssemblySymbol assemblySymbol:
                {
                    id = new SerializableDeclarationId( $"{_assemblyPrefix}{assemblySymbol.Identity}" );

                    return true;
                }

            default:
                var documentationId = DocumentationCommentId.CreateDeclarationId( symbol );

                if ( targetKind == DeclarationRefTargetKind.Default )
                {
                    id = new SerializableDeclarationId( documentationId );
                }
                else
                {
                    id = new SerializableDeclarationId( $"{documentationId};{targetKind}" );
                }

                return true;
        }
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
        else if ( id.Id.StartsWith( _assemblyPrefix, StringComparison.OrdinalIgnoreCase ) )
        {
            if ( !AssemblyIdentity.TryParseDisplayName( id.Id.Substring( _assemblyPrefix.Length ), out var assemblyIdentity ) )
            {
                throw new AssertionFailedException( $"Cannot parse the id '{id.Id}'." );
            }

            if ( compilation.Assembly.Identity.Equals( assemblyIdentity ) )
            {
                return compilation.Assembly;
            }
            else
            {
                return compilation.SourceModule.ReferencedAssemblySymbols.SingleOrDefault( a => a.Identity.Equals( assemblyIdentity ) );
            }
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
        else if ( id.Id.StartsWith( _assemblyPrefix, StringComparison.OrdinalIgnoreCase ) )
        {
            if ( !AssemblyIdentity.TryParseDisplayName( id.Id.Substring( _assemblyPrefix.Length ), out var assemblyIdentity ) )
            {
                throw new AssertionFailedException( $"Cannot parse the id '{id.Id}'." );
            }

            return compilation.Factory.GetAssembly( assemblyIdentity );
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