// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

internal abstract class CompilationBoundRef<T> : BaseRef<T>, ICompilationBoundRefImpl
    where T : class, ICompilationElement
{
    public sealed override bool IsPortable => false;

    public abstract CompilationContext CompilationContext { get; }

    /// <summary>
    /// Gets all <see cref="AttributeData"/> on the target of the reference without resolving the reference to
    /// the code model.
    /// </summary>
    public (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) GetAttributeData()
    {
        if ( this.TargetKind != RefTargetKind.Default )
        {
            var baseSymbol = this.GetSymbolIgnoringKind();

            switch ( this.TargetKind )
            {
                case RefTargetKind.Return when baseSymbol is IMethodSymbol method:
                    return (method.GetReturnTypeAttributes(), method);

                case RefTargetKind.Field when baseSymbol is IEventSymbol @event:
                    // Roslyn does not expose the backing field of an event, so we don't have access to its attributes.
                    return (ImmutableArray<AttributeData>.Empty, @event);
            }

            // Fallback to the default GetSymbol implementation.
        }

        var symbol = this.GetSymbol( this.CompilationContext, true );

        return (symbol.GetAttributes(), symbol);
    }

    [Memo]
    private StringRef<T> CompilationNeutralRef => new( this.ToSerializableId().Id );

    public sealed override IRef<T> ToPortable() => this.CompilationNeutralRef;

    public override SerializableDeclarationId ToSerializableId()
    {
        var symbol = this.GetSymbolIgnoringKind( true );

        return symbol.GetSerializableId( this.TargetKind );
    }

    protected override ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => this.GetSymbolWithKind( this.GetSymbolIgnoringKind( ignoreAssemblyKey ) );

    protected abstract ISymbol GetSymbolIgnoringKind( bool ignoreAssemblyKey = false );

    public override ISymbol GetClosestContainingSymbol( CompilationContext compilationContext )
    {
        Invariant.Assert( compilationContext == this.CompilationContext );

        return this.GetSymbolIgnoringKind();
    }

    private ISymbol GetSymbolWithKind( ISymbol symbol )
        => this.TargetKind switch
        {
            RefTargetKind.Assembly when symbol is IAssemblySymbol => symbol,
            RefTargetKind.Module when symbol is IModuleSymbol => symbol,
            RefTargetKind.NamedType when symbol is INamedTypeSymbol => symbol,
            RefTargetKind.Default => symbol,
            RefTargetKind.Return => throw new InvalidOperationException( "Cannot get a symbol for the method return parameter." ),
            RefTargetKind.Field when symbol is IPropertySymbol property => property.GetBackingField().AssertSymbolNotNull(),
            RefTargetKind.Field when symbol is IEventSymbol => throw new InvalidOperationException( "Cannot get the underlying field of an event." ),
            RefTargetKind.Parameter when symbol is IPropertySymbol property => property.SetMethod.AssertSymbolNotNull().Parameters[0],
            RefTargetKind.Parameter when symbol is IMethodSymbol method => method.Parameters[0],
            RefTargetKind.Property when symbol is IParameterSymbol parameter => parameter.ContainingType.GetMembers( symbol.Name )
                .OfType<IPropertySymbol>()
                .Single(),
            _ => throw new AssertionFailedException( $"Don't know how to get the symbol kind {this.TargetKind} for a {symbol.Kind}." )
        };
}