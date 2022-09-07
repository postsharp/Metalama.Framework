// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking
{
    internal class ResolvedAspectReference
    {
        /// <summary>
        /// Gets the semantic that contains the reference.
        /// </summary>
        public IntermediateSymbolSemantic<IMethodSymbol> ContainingSemantic { get; }

        /// <summary>
        /// Gets the symbol the reference was originally pointing to.
        /// </summary>
        public ISymbol OriginalSymbol { get; }

        /// <summary>
        /// Gets the symbol semantic that is the target of the reference (C# declaration, i.e. method, property or event).
        /// </summary>
        public IntermediateSymbolSemantic ResolvedSemantic { get; }

        /// <summary>
        /// Gets the symbol semantic for the target body (always a method).
        /// </summary>
        public IntermediateSymbolSemantic<IMethodSymbol> ResolvedSemanticBody
        {
            get
                => (this.ResolvedSemantic, this.TargetKind) switch
                {
                    ({ Symbol: IMethodSymbol method }, AspectReferenceTargetKind.Self) =>
                        method.ToSemantic( this.ResolvedSemantic.Kind ),
                    ({ Symbol: IPropertySymbol property }, AspectReferenceTargetKind.PropertyGetAccessor ) =>
                        property.GetMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                    ({ Symbol: IPropertySymbol property }, AspectReferenceTargetKind.PropertySetAccessor ) =>
                        property.SetMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                    ({ Symbol: IEventSymbol @event }, AspectReferenceTargetKind.EventAddAccessor ) =>
                        @event.AddMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                    ({ Symbol: IEventSymbol @event }, AspectReferenceTargetKind.EventRemoveAccessor ) =>
                        @event.RemoveMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                    _ => throw new AssertionFailedException(),
                };
        }

        /// <summary>
        /// Gets the annotated expression. This is for convenience in inliners which always work with expressions.
        /// </summary>
        public ExpressionSyntax SourceExpression => this.SourceNode as ExpressionSyntax ?? throw new AssertionFailedException();

        /// <summary>
        /// Gets the annotated expression.
        /// </summary>
        public SyntaxNode SourceNode { get; }

        /// <summary>
        /// Gets a value indicating whether the reference is inlineable.
        /// </summary>
        public bool IsInlineable { get; }

        /// <summary>
        /// Gets the target kind of the aspect reference.
        /// </summary>
        public AspectReferenceTargetKind TargetKind { get; }

        public ResolvedAspectReference(
            IntermediateSymbolSemantic<IMethodSymbol> containingSemantic,
            ISymbol originalSymbol,
            IntermediateSymbolSemantic resolvedSemantic,
            SyntaxNode sourceNode,
            AspectReferenceTargetKind targetKind,
            bool isInlineable )
        {
            Invariant.AssertNot( containingSemantic.Kind != IntermediateSymbolSemanticKind.Final && sourceNode is not ExpressionSyntax );
            Invariant.AssertNot( resolvedSemantic.Symbol is IMethodSymbol { MethodKind: not MethodKind.Ordinary and not MethodKind.ExplicitInterfaceImplementation and not MethodKind.Destructor and not MethodKind.UserDefinedOperator and not MethodKind.Conversion } );

            this.ContainingSemantic = containingSemantic;
            this.OriginalSymbol = originalSymbol;
            this.ResolvedSemantic = resolvedSemantic;
            this.SourceNode = sourceNode;
            this.IsInlineable = isInlineable;
            this.TargetKind = targetKind;
        }

        public override string ToString()
        {
            return $"{this.ContainingSemantic} ({(this.SourceNode is ExpressionSyntax ? this.SourceNode : "not expression")}) -> {this.ResolvedSemantic}";
        }
    }
}