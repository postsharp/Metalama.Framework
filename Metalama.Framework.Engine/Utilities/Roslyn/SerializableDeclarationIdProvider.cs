// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
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

    public static SerializableDeclarationId GetSerializableId( this ISymbol symbol ) => symbol.GetSerializableId( DeclarationRefTargetKind.Default );

    internal static SerializableDeclarationId GetSerializableId( this ISymbol symbol, DeclarationRefTargetKind targetKind )
    {
        if ( !TryGetSerializableId( symbol, targetKind, out var id ) )
        {
            throw new ArgumentException( $"Cannot create a SerializableDeclarationId for '{symbol}'.", nameof(symbol) );
        }

        return id;
    }

    public static bool TryGetSerializableId( this ISymbol? symbol, out SerializableDeclarationId id )
        => TryGetSerializableId( symbol, DeclarationRefTargetKind.Default, out id );

    private static bool TryGetSerializableId( this ISymbol? symbol, DeclarationRefTargetKind targetKind, out SerializableDeclarationId id )
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

    internal static SerializableDeclarationId GetSerializableId( this IDeclaration declaration )
        => declaration.GetSerializableId( DeclarationRefTargetKind.Default );

    internal static SerializableDeclarationId GetSerializableId( this IDeclaration declaration, DeclarationRefTargetKind targetKind )
    {
        if ( !TryGetSerializableId( declaration, targetKind, out var id ) )
        {
            throw new ArgumentException( $"Cannot create a SerializableDeclarationId for '{declaration}'.", nameof(declaration) );
        }

        return id;
    }

    public static bool TryGetSerializableId( this IDeclaration? declaration, out SerializableDeclarationId id )
        => TryGetSerializableId( declaration, DeclarationRefTargetKind.Default, out id );

    private static bool TryGetSerializableId( this IDeclaration? declaration, DeclarationRefTargetKind targetKind, out SerializableDeclarationId id )
    {
        switch ( declaration )
        {
            case null:

                id = default;

                return false;

            case IParameter { IsReturnParameter: true } parameter:
                return TryGetSerializableId( parameter.DeclaringMember, DeclarationRefTargetKind.Return, out id );

            case IParameter { ContainingDeclaration.ContainingDeclaration: IField } parameter:
                return TryGetSerializableId( parameter.ContainingDeclaration, DeclarationRefTargetKind.Parameter, out id );

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
                return TryGetSerializableId( eventRaisePseudoAccessor.DeclaringMember, DeclarationRefTargetKind.EventRaise, out id );

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

    /// <summary>Gets the <see cref="SerializableDeclarationId"/> for the declaration as it appears in the unmodified source code.</summary>
    /// <remarks>This is relevant in the case of constructor parameter introduction, which alter the serializable ID of the constructor.</remarks>
    public static SerializableDeclarationId GetSourceSerializableId( this IDeclaration declaration )
        => declaration.GetSymbol()?.GetSerializableId() ?? declaration.ToSerializableId();

    [PublicAPI]
    public static ISymbol ResolveToSymbol( this SerializableDeclarationId id, Compilation compilation )
    {
        // Note that the symbol resolution can fail for methods when the method signature contains a type from a missing assembly.

        var symbol = id.ResolveToSymbolOrNull( compilation );

        if ( symbol == null )
        {
            throw new AssertionFailedException( $"Cannot get a symbol for '{id}'." );
        }

        return symbol;
    }

    public static ISymbol? ResolveToSymbolOrNull( this SerializableDeclarationId id, Compilation compilation )
    {
        var symbol = id.ResolveToSymbolOrNull( compilation, out var isReturnParameter );

        return isReturnParameter ? null : symbol;
    }

    /// <remarks>This overload is only used for aspect targets, so it doesn't need to handle the other target kinds.</remarks>
    public static ISymbol? ResolveToSymbolOrNull( this SerializableDeclarationId id, Compilation compilation, out bool isReturnParameter )
    {
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

            if ( kind == nameof(DeclarationRefTargetKind.Return) )
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

            return DocumentationCommentId.GetFirstSymbolForDeclarationId( id.ToString(), compilation );
        }
    }

    internal static IDeclaration? ResolveToDeclaration( this SerializableDeclarationId id, CompilationModel compilation )
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
                (IMethod method, nameof(DeclarationRefTargetKind.Return)) => method.ReturnParameter,
                (IField field, nameof(DeclarationRefTargetKind.PropertyGet)) => field.GetMethod,
                (IField field, nameof(DeclarationRefTargetKind.PropertySet)) => field.SetMethod,
                (IField field, nameof(DeclarationRefTargetKind.PropertySetParameter)) => field.SetMethod?.Parameters[0],
                (IField field, nameof(DeclarationRefTargetKind.PropertyGetReturnParameter)) => field.GetMethod?.ReturnParameter,
                (IField field, nameof(DeclarationRefTargetKind.PropertySetReturnParameter)) => field.SetMethod?.ReturnParameter,
                (IEvent @event, nameof(DeclarationRefTargetKind.EventRaise)) => @event.RaiseMethod,
                (IEvent @event, nameof(DeclarationRefTargetKind.EventRaiseParameter)) => @event.RaiseMethod?.Parameters[0],
                (IEvent @event, nameof(DeclarationRefTargetKind.EventRaiseReturnParameter)) => @event.RaiseMethod?.ReturnParameter,
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
            return DeclarationDocumentationCommentId.GetFirstDeclarationForDeclarationId( id.ToString(), compilation );
        }
    }
}