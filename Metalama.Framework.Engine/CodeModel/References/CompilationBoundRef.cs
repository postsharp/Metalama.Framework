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

/// <summary>
/// Specialization of <see cref="BaseRef{T}"/> for references bound to a <see cref="CompilationContext"/>.
/// </summary>
internal abstract class CompilationBoundRef<T> : BaseRef<T>, ICompilationBoundRefImpl
    where T : class, ICompilationElement
{
    public sealed override bool IsDurable => false;

    public abstract CompilationContext CompilationContext { get; }

    public abstract ICompilationBoundRefImpl WithGenericContext( GenericContext genericContext );

    public abstract IRefStrategy Strategy { get; }

    /// <summary>
    /// Gets all <see cref="AttributeData"/> on the target of the reference without resolving the reference to
    /// the code model.
    /// </summary>
    public ResolvedAttributeRef GetAttributeData()
    {
        switch ( this.TargetKind )
        {
            case RefTargetKind.Return when this.GetSymbolIgnoringRefKind( this.CompilationContext ) is IMethodSymbol method:
                return new ResolvedAttributeRef( method.GetReturnTypeAttributes(), method, RefTargetKind.Return );

            case RefTargetKind.Field when this.GetSymbolIgnoringRefKind( this.CompilationContext ) is IEventSymbol @event:
                // Roslyn does not expose the backing field of an event, so we don't have access to its attributes.
                return new ResolvedAttributeRef( ImmutableArray<AttributeData>.Empty, @event, RefTargetKind.Field );

            default:
                var symbol = this.GetSymbol( this.CompilationContext, true );

                return new ResolvedAttributeRef( symbol.GetAttributes(), symbol, RefTargetKind.Default );
        }
    }

    public abstract bool IsDefinition { get; }

    public abstract IRef Definition { get; }

    [Memo]
    private DeclarationIdRef<T> CompilationNeutralRef => new( this.ToSerializableId() );

    public sealed override IDurableRef<T> ToDurable() => this.CompilationNeutralRef;

    public override SerializableDeclarationId ToSerializableId() => this.ToSerializableId( this.CompilationContext );

    public override SerializableDeclarationId ToSerializableId( CompilationContext compilationContext )
    {
        var symbol = this.GetSymbolIgnoringRefKind( compilationContext, true );

        return symbol.GetSerializableId( this.TargetKind );
    }

    protected override ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => this.ApplyRefKind( this.GetSymbolIgnoringRefKind( compilationContext, ignoreAssemblyKey ) );

    protected abstract ISymbol GetSymbolIgnoringRefKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false );

    public override ISymbol GetClosestContainingSymbol( CompilationContext compilationContext )
    {
        Invariant.Assert( compilationContext == this.CompilationContext );

        return this.GetSymbolIgnoringRefKind( this.CompilationContext );
    }
    
    private ISymbol ApplyRefKind( ISymbol symbol )
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