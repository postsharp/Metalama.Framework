// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Represents a semantic for a symbol in the intermediate compilation. It is virtualized, able to represent accessors that are not part of the intermediate compilation.
    /// </summary>
    internal readonly struct IntermediateSymbolSemantic : IEquatable<IntermediateSymbolSemantic>
    {
        /// <summary>
        /// Gets the main symbol.
        /// </summary>
        public ISymbol Symbol { get; }

        /// <summary>
        /// Gets the semantic kind (i.e. "version" associated with the symbol).
        /// </summary>
        public IntermediateSymbolSemanticKind Kind { get; }

        /// <summary>
        /// Gets the target of the semantic, i.e. the declaration itself or an accessor.
        /// </summary>
        public IntermediateSymbolSemanticTargetKind Target { get; }

        public IntermediateSymbolSemantic( ISymbol symbol, IntermediateSymbolSemanticKind kind, IntermediateSymbolSemanticTargetKind target )
        {
            Invariant.AssertNot( symbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } );
            Invariant.AssertNot( symbol is IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise } );
            Invariant.AssertNot( symbol is IMethodSymbol && target != IntermediateSymbolSemanticTargetKind.Self );

            this.Symbol = symbol;
            this.Kind = kind;
            this.Target = target;
        }

        public static IntermediateSymbolSemantic Create( ISymbol symbol, IntermediateSymbolSemanticKind kind )
            => symbol switch
            {
                IMethodSymbol { MethodKind: MethodKind.PropertySet } methodSymbol
                    => new IntermediateSymbolSemantic( methodSymbol.AssociatedSymbol, kind, IntermediateSymbolSemanticTargetKind.PropertySet ),
                IMethodSymbol { MethodKind: MethodKind.PropertyGet } methodSymbol
                    => new IntermediateSymbolSemantic( methodSymbol.AssociatedSymbol, kind, IntermediateSymbolSemanticTargetKind.PropertyGet ),
                IMethodSymbol { MethodKind: MethodKind.EventAdd } methodSymbol
                    => new IntermediateSymbolSemantic( methodSymbol.AssociatedSymbol, kind, IntermediateSymbolSemanticTargetKind.EventAdd ),
                IMethodSymbol { MethodKind: MethodKind.EventRemove } methodSymbol
                    => new IntermediateSymbolSemantic( methodSymbol.AssociatedSymbol, kind, IntermediateSymbolSemanticTargetKind.EventRemove ),
                IMethodSymbol { MethodKind: MethodKind.EventRaise } methodSymbol
                    => new IntermediateSymbolSemantic( methodSymbol.AssociatedSymbol, kind, IntermediateSymbolSemanticTargetKind.EventRaise ),
                IMethodSymbol or IPropertySymbol or IEventSymbol
                    => new IntermediateSymbolSemantic( symbol, kind, IntermediateSymbolSemanticTargetKind.Self ),
                _ => throw new AssertionFailedException(),
            };

        public bool Equals( IntermediateSymbolSemantic other )
        {
            return StructuralSymbolComparer.Default.Equals( this.Symbol, other.Symbol )
                   && other.Kind == this.Kind
                   && other.Target == this.Target;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StructuralSymbolComparer.Default.GetHashCode( this.Symbol ),
                this.Kind,
                this.Target );
        }

        public IntermediateSymbolSemantic<TSymbol> ToTyped<TSymbol>()
            where TSymbol : ISymbol
        {
            ISymbol symbol = (this.Symbol, this.Target) switch
            {
                (IPropertySymbol property, IntermediateSymbolSemanticTargetKind.Self ) => property,
                (IPropertySymbol property, IntermediateSymbolSemanticTargetKind.PropertyGet ) => property.GetMethod.AssertNotNull(),
                (IPropertySymbol property, IntermediateSymbolSemanticTargetKind.PropertySet ) => property.SetMethod.AssertNotNull(),
                (IEventSymbol @event, IntermediateSymbolSemanticTargetKind.Self ) => @event,
                (IEventSymbol @event, IntermediateSymbolSemanticTargetKind.EventAdd ) => @event.AddMethod.AssertNotNull(),
                (IEventSymbol @event, IntermediateSymbolSemanticTargetKind.EventRemove ) => @event.RemoveMethod.AssertNotNull(),
                (IEventSymbol @event, IntermediateSymbolSemanticTargetKind.EventRaise ) => @event.RaiseMethod.AssertNotNull(),
                (IMethodSymbol method, IntermediateSymbolSemanticTargetKind.Self ) => method,
                _ => throw new AssertionFailedException(),
            };

            return new IntermediateSymbolSemantic<TSymbol>( (TSymbol) symbol, this.Kind );
        }

        public IntermediateSymbolSemantic WithSymbol( ISymbol symbol )
        {
            return Create( symbol, this.Kind );
        }

        public IntermediateSymbolSemantic<IMethodSymbol> WithSymbol( IMethodSymbol symbol )
        {
            return Create( symbol, this.Kind ).ToTyped<IMethodSymbol>();
        }

        public IntermediateSymbolSemantic WithKind( IntermediateSymbolSemanticKind kind )
        {
            return new IntermediateSymbolSemantic( this.Symbol, kind, this.Target );
        }

        public override string ToString()
        {
            // Coverage: ignore (useful for debugging)
            return $"({this.Kind}, {this.Target}, {this.Symbol})";
        }
    }

    internal readonly struct IntermediateSymbolSemantic<TSymbol> : IEquatable<IntermediateSymbolSemantic<TSymbol>>
        where TSymbol : ISymbol
    {
        public TSymbol Symbol { get; }

        public IntermediateSymbolSemanticKind Kind { get; }

        public IntermediateSymbolSemantic( TSymbol symbol, IntermediateSymbolSemanticKind semantic )
        {
            this.Symbol = symbol;
            this.Kind = semantic;
        }

        public bool Equals( IntermediateSymbolSemantic<TSymbol> other )
        {
            return StructuralSymbolComparer.Default.Equals( this.Symbol, other.Symbol )
                   && other.Kind == this.Kind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StructuralSymbolComparer.Default.GetHashCode( this.Symbol ),
                this.Kind );
        }

        public static implicit operator IntermediateSymbolSemantic( IntermediateSymbolSemantic<TSymbol> value )
        {
            return IntermediateSymbolSemantic.Create( value.Symbol, value.Kind );
        }

        public IntermediateSymbolSemantic WithSymbol( ISymbol symbol )
        {
            return new IntermediateSymbolSemantic( symbol, this.Kind, IntermediateSymbolSemanticTargetKind.Self );
        }

        public IntermediateSymbolSemantic<TSymbol> WithSymbol( TSymbol symbol )
        {
            return new IntermediateSymbolSemantic<TSymbol>( symbol, this.Kind );
        }

        public IntermediateSymbolSemantic<TSymbol> WithKind( IntermediateSymbolSemanticKind kind )
        {
            return new IntermediateSymbolSemantic<TSymbol>( this.Symbol, kind );
        }

        public override string ToString()
        {
            // Coverage: ignore (useful for debugging)
            return $"({this.Kind}, {this.Symbol})";
        }
    }
}