// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking
{
    internal static class IntermediateSymbolSemanticExtensions
    {
        public static IntermediateSymbolSemantic<T> ToSemantic<T>( this T symbol, IntermediateSymbolSemanticKind kind )
            where T : ISymbol
        {
            return ((ISymbol)symbol).ToSemantic( kind ).ToTyped<T>();
        }

        public static IntermediateSymbolSemantic ToSemantic( this ISymbol symbol, IntermediateSymbolSemanticKind kind )
            => IntermediateSymbolSemantic.Create( symbol, kind );

        public static AspectReferenceTarget ToAspectReferenceTarget( this IntermediateSymbolSemantic semantic )
        {
            return new AspectReferenceTarget( semantic.Symbol, semantic.Kind, semantic.Target.ToAspectReferenceTargetKind() );
        }
        public static AspectReferenceTarget ToAspectReferenceTarget( this IntermediateSymbolSemantic semantic, AspectReferenceTargetKind targetKind )
        {
            return new AspectReferenceTarget( semantic.Symbol, semantic.Kind, targetKind );
        }

        public static AspectReferenceTarget ToAspectReferenceTarget( this IntermediateSymbolSemantic<IMethodSymbol> target )
        {
            return new AspectReferenceTarget( target.Symbol, target.Kind, AspectReferenceTargetKind.Self );
        }

        public static AspectReferenceTarget ToAspectReferenceTarget(
            this IntermediateSymbolSemantic<IPropertySymbol> property,
            AspectReferenceTargetKind targetKind )
        {
            Invariant.Assert( targetKind is AspectReferenceTargetKind.PropertyGetAccessor or AspectReferenceTargetKind.PropertySetAccessor );

            return new AspectReferenceTarget( property.Symbol, property.Kind, targetKind );
        }

        public static AspectReferenceTarget ToAspectReferenceTarget(
            this IntermediateSymbolSemantic<IEventSymbol> @event,
            AspectReferenceTargetKind targetKind )
        {
            Invariant.Assert( targetKind is AspectReferenceTargetKind.EventAddAccessor or AspectReferenceTargetKind.EventRemoveAccessor );

            return new AspectReferenceTarget( @event.Symbol, @event.Kind, targetKind );
        }

        public static IntermediateSymbolSemanticTargetKind ToSemanticTargetKind( this AspectReferenceTargetKind kind )
            => kind switch
            {
                AspectReferenceTargetKind.Self => IntermediateSymbolSemanticTargetKind.Self,
                AspectReferenceTargetKind.PropertyGetAccessor => IntermediateSymbolSemanticTargetKind.PropertyGet,
                AspectReferenceTargetKind.PropertySetAccessor => IntermediateSymbolSemanticTargetKind.PropertySet,
                AspectReferenceTargetKind.EventAddAccessor => IntermediateSymbolSemanticTargetKind.EventAdd,
                AspectReferenceTargetKind.EventRemoveAccessor => IntermediateSymbolSemanticTargetKind.EventRemove,
                AspectReferenceTargetKind.EventRaiseAccessor => IntermediateSymbolSemanticTargetKind.EventRaise,
                _ => throw new AssertionFailedException(),
            };

        public static AspectReferenceTargetKind ToAspectReferenceTargetKind( this IntermediateSymbolSemanticTargetKind kind )
            => kind switch
            {
                IntermediateSymbolSemanticTargetKind.Self => AspectReferenceTargetKind.Self,
                IntermediateSymbolSemanticTargetKind.PropertyGet => AspectReferenceTargetKind.PropertyGetAccessor,
                IntermediateSymbolSemanticTargetKind.PropertySet => AspectReferenceTargetKind.PropertySetAccessor,
                IntermediateSymbolSemanticTargetKind.EventAdd => AspectReferenceTargetKind.EventAddAccessor,
                IntermediateSymbolSemanticTargetKind.EventRemove => AspectReferenceTargetKind.EventRemoveAccessor,
                IntermediateSymbolSemanticTargetKind.EventRaise => AspectReferenceTargetKind.EventRaiseAccessor,
                _ => throw new AssertionFailedException(),
            };
    }
}