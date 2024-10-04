// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using MetalamaMethodKind = Metalama.Framework.Code.MethodKind;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static class SerializableDeclarationIdProvider
{
    private const string _assemblyPrefix = "Assembly:";

    private static readonly char[] _separators = new[] { ';', '=' };

    public static SerializableDeclarationId GetSerializableId( this ISymbol symbol ) => symbol.GetSerializableId( RefTargetKind.Default );

    internal static SerializableDeclarationId GetSerializableId( this ISymbol symbol, RefTargetKind targetKind )
    {
        if ( !TryGetSerializableId( symbol, targetKind, out var id ) )
        {
            throw new ArgumentException( $"Cannot create a SerializableDeclarationId for '{symbol}'.", nameof(symbol) );
        }

        return id;
    }

    public static bool TryGetSerializableId( this ISymbol? symbol, out SerializableDeclarationId id )
        => TryGetSerializableId( symbol, RefTargetKind.Default, out id );

    private static bool TryGetSerializableId( this ISymbol? symbol, RefTargetKind targetKind, out SerializableDeclarationId id )
    {
        switch ( symbol )
        {
            case null:
            case ILocalSymbol:
            case IMethodSymbol
            {
                MethodKind: RoslynMethodKind.LocalFunction or RoslynMethodKind.AnonymousFunction
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

            case IModuleSymbol:
                {
                    id = default;

                    return false;
                }

            case INamedTypeSymbol:
                goto default;

            case ITypeSymbol typeSymbol:
                id = new SerializableDeclarationId( typeSymbol.GetSerializableTypeId().Id );

                return true;

            default:
                switch ( symbol.Kind )
                {
                    case SymbolKind.NamedType:
                    case SymbolKind.Method:
                    case SymbolKind.Field:
                    case SymbolKind.Assembly:
                    case SymbolKind.Event:
                    case SymbolKind.Namespace:
                    case SymbolKind.Parameter:
                    case SymbolKind.Property:
                    case SymbolKind.TypeParameter:
                        {
                            var documentationId = DocumentationCommentId.CreateDeclarationId( symbol );

                            if ( targetKind == RefTargetKind.Default )
                            {
                                id = new SerializableDeclarationId( documentationId );
                            }
                            else
                            {
                                id = new SerializableDeclarationId( $"{documentationId};{targetKind}" );
                            }

                            return true;
                        }

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(symbol),
                            $"Cannot create a SerializableDeclarationId for '{symbol}' because it is a {symbol.Kind}." );
                }
        }
    }

    internal static SerializableDeclarationId GetSerializableId( this IDeclaration declaration ) => declaration.GetSerializableId( RefTargetKind.Default );

    internal static SerializableDeclarationId GetSerializableId( this IDeclaration declaration, RefTargetKind targetKind )
    {
        if ( !TryGetSerializableId( declaration, targetKind, out var id ) )
        {
            throw new ArgumentException( $"Cannot create a SerializableDeclarationId for '{declaration}'.", nameof(declaration) );
        }

        return id;
    }

    public static bool TryGetSerializableId( this IDeclaration? declaration, out SerializableDeclarationId id )
        => TryGetSerializableId( declaration, RefTargetKind.Default, out id );

    private static bool TryGetSerializableId( this IDeclaration? declaration, RefTargetKind targetKind, out SerializableDeclarationId id )
    {
        switch ( declaration )
        {
            case null:

                id = default;

                return false;

            case IParameter { IsReturnParameter: true } parameter:
                return TryGetSerializableId( parameter.DeclaringMember, RefTargetKind.Return, out id );

            case IParameter { ContainingDeclaration.ContainingDeclaration: IField } parameter:
                return TryGetSerializableId( parameter.ContainingDeclaration, RefTargetKind.Parameter, out id );

            case IParameter parameter:
                {
                    var parentId = DeclarationDocumentationCommentId.CreateDeclarationId( parameter.DeclaringMember ).AssertNotNull();

                    id = new SerializableDeclarationId( $"{parentId};Parameter={parameter.Index}" );

                    return true;
                }

            case ITypeParameter typeParameter:
                {
                    var parentId = DeclarationDocumentationCommentId.CreateDeclarationId( typeParameter.ContainingDeclaration! ).AssertNotNull();

                    id = new SerializableDeclarationId( $"{parentId};TypeParameter={typeParameter.Index}" );

                    return true;
                }

            case IAssembly assembly:
                {
                    id = new SerializableDeclarationId( $"{_assemblyPrefix}{assembly.Identity}" );

                    return true;
                }

            case IMethod { ContainingDeclaration: IField } fieldPseudoAccessor:
                return TryGetSerializableId(
                    fieldPseudoAccessor.DeclaringMember,
                    fieldPseudoAccessor.MethodKind.ToDeclarationRefTargetKind( targetKind ),
                    out id );

            case IMethod { ContainingDeclaration: IEvent, MethodKind: MetalamaMethodKind.EventRaise } eventRaisePseudoAccessor:
                return TryGetSerializableId( eventRaisePseudoAccessor.DeclaringMember, RefTargetKind.EventRaise, out id );

            default:
                string documentationId;

                try
                {
                    documentationId = DeclarationDocumentationCommentId.CreateDeclarationId( declaration );
                }
                catch ( InvalidOperationException exception )
                {
                    throw new InvalidOperationException(
                        $"Cannot get a DeclarationDocumentationCommentId for '{declaration}' ({declaration.DeclarationKind}).",
                        exception );
                }

                if ( targetKind == RefTargetKind.Default )
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

    /// <summary>Gets the <see cref="SerializableDeclarationId"/> for the declaration as it appears in the unmodified source code.</summary>
    /// <remarks>This is relevant in the case of constructor parameter introduction, which alter the serializable ID of the constructor.</remarks>
    public static SerializableDeclarationId GetSourceSerializableId( this IDeclaration declaration )
        => declaration.GetSymbol()?.GetSerializableId() ?? declaration.ToSerializableId();

    [PublicAPI]
    public static ISymbol ResolveToSymbol( this SerializableDeclarationId id, CompilationContext compilationContext )
    {
        // Note that the symbol resolution can fail for methods when the method signature contains a type from a missing assembly.

        var symbol = id.ResolveToSymbolOrNull( compilationContext )
                     ??
                     throw new AssertionFailedException( $"Cannot get a symbol for '{id}'." );

        return symbol;
    }

    public static ISymbol? ResolveToSymbolOrNull( this SerializableDeclarationId id, CompilationContext compilationContext )
    {
        var symbol = id.ResolveToSymbolOrNull( compilationContext, out var isReturnParameter );

        return isReturnParameter ? null : symbol;
    }

    public static ISymbol? ResolveToSymbolOrNull( this SerializableDeclarationId id, CompilationContext compilationContext, out bool isReturnParameter )
    {
        var compilation = compilationContext.Compilation;

        isReturnParameter = false;

        var indexOfAt = id.Id.IndexOfOrdinal( ';' );

        if ( indexOfAt > 0 )
        {
            // We have a parameter or a type parameter.

            var parts = id.Id.Split( _separators );

            var parentId = parts[0];
            var kind = parts[1];
            var ordinal = parts.Length == 3 ? int.Parse( parts[2], CultureInfo.InvariantCulture ) : -1;

            var parent = DocumentationCommentId.GetFirstSymbolForDeclarationId( parentId, compilation );

            if ( kind == nameof(RefTargetKind.Return) )
            {
                isReturnParameter = true;

                return parent;
            }

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
            // Special case for the global namespace that's not handled by GetFirstSymbolForDeclarationId, see https://github.com/dotnet/roslyn/issues/66976.
            if ( id.Id == "N:" )
            {
                return compilation.Assembly.GlobalNamespace;
            }
            else if ( id.Id.StartsWith( SerializableTypeId.Prefix, StringComparison.Ordinal ) )
            {
                if ( !compilationContext.SerializableTypeIdResolver.TryResolveId( new SerializableTypeId( id.Id ), out var typeSymbol ) )
                {
                    return null;
                }
                else
                {
                    return typeSymbol;
                }
            }

            return DocumentationCommentId.GetFirstSymbolForDeclarationId( id.ToString(), compilation );
        }
    }

    internal static ICompilationElement? ResolveToDeclaration( this SerializableDeclarationId id, CompilationModel compilation )
    {
        var indexOfAt = id.Id.IndexOfOrdinal( ';' );

        if ( indexOfAt > 0 )
        {
            // We have a parameter or a type parameter.

            var parts = id.Id.Split( _separators );

            var parentId = parts[0];
            var kind = parts[1];
            var ordinal = parts.Length == 3 ? int.Parse( parts[2], CultureInfo.InvariantCulture ) : -1;

            var parent = DeclarationDocumentationCommentId.GetFirstDeclarationForDeclarationId( parentId, compilation );

            return (parent, kind) switch
            {
                (null, _) => null,
                (IHasParameters method, "Parameter") => method.Parameters[ordinal],
                (IGeneric generic, "TypeParameter") => generic.TypeParameters[ordinal],
                (IMethod method, nameof(RefTargetKind.Return)) => method.ReturnParameter,
                (IField field, nameof(RefTargetKind.PropertyGet)) => field.GetMethod,
                (IField field, nameof(RefTargetKind.PropertySet)) => field.SetMethod,
                (IField field, nameof(RefTargetKind.PropertySetParameter)) => field.SetMethod?.Parameters[0],
                (IField field, nameof(RefTargetKind.PropertyGetReturnParameter)) => field.GetMethod?.ReturnParameter,
                (IField field, nameof(RefTargetKind.PropertySetReturnParameter)) => field.SetMethod?.ReturnParameter,
                (IEvent @event, nameof(RefTargetKind.EventRaise)) => @event.RaiseMethod,
                (IEvent @event, nameof(RefTargetKind.EventRaiseParameter)) => @event.RaiseMethod?.Parameters[0],
                (IEvent @event, nameof(RefTargetKind.EventRaiseReturnParameter)) => @event.RaiseMethod?.ReturnParameter,
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
        else if ( id.Id.StartsWith( SerializableTypeId.Prefix, StringComparison.Ordinal ) )
        {
            if ( !compilation.CompilationContext.SerializableTypeIdResolver.TryResolveId( new SerializableTypeId( id.Id ), out var typeSymbol ) )
            {
                return null;
            }
            else
            {
                return compilation.Factory.GetIType( typeSymbol );
            }
        }
        else
        {
            return DeclarationDocumentationCommentId.GetFirstDeclarationForDeclarationId( id.ToString(), compilation );
        }
    }
}