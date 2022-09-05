// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal sealed class InliningSpecification
    {
        private readonly int _inliningId;
        private readonly int? _parentInliningId;

        /// <summary>
        /// Gets the semantic which is a the destination of inlining (the top-level semantic which will contain the body).
        /// This allows us to discern between inlinings of one aspect reference into two separate bodies (used for deconstructed auto properties).
        /// </summary>
        public IntermediateSymbolSemantic<IMethodSymbol> DestinationSemantic { get; }

        /// <summary>
        /// Gets the identifier of inlining within the destination semantic.
        /// This allows us to inline one aspect reference twice into one target body (used for deconstructed auto properties).
        /// </summary>
        public InliningContextIdentifier ContextIdentifier => new InliningContextIdentifier( this.DestinationSemantic, this._inliningId );

        /// <summary>
        /// Gets the identifier of inlining within the destination semantic.
        /// This allows us to inline one aspect reference twice into one target body (used for deconstructed auto properties).
        /// </summary>
        public InliningContextIdentifier ParentContextIdentifier => new InliningContextIdentifier( this.DestinationSemantic, this._parentInliningId );

        /// <summary>
        /// Gets the aspect reference to be inlined.
        /// </summary>
        public ResolvedAspectReference AspectReference { get; }

        /// <summary>
        /// Gets the inliner that created this specification.
        /// </summary>
        public Inliner Inliner { get; }

        /// <summary>
        /// Gets the statement replaced by inlining.
        /// </summary>
        public SyntaxNode ReplacedRootNode { get; }

        /// <summary>
        /// Gets a value indicating whether simple inlining (no return transformation) may be used.
        /// </summary>
        public bool UseSimpleInlining { get; }

        /// <summary>
        /// Gets a value indicating whether this inlining requires the return variable to be declared.
        /// </summary>
        public bool DeclareReturnVariable { get; }

        /// <summary>
        /// Gets the return variable identifier.
        /// </summary>
        public string? ReturnVariableIdentifier { get; }

        /// <summary>
        /// Gets the return label indentifier.
        /// </summary>
        public string? ReturnLabelIdentifier { get; }

        /// <summary>
        /// Gets a symbol from intermediate compilation that will be inlined.
        /// </summary>
        public IntermediateSymbolSemantic<IMethodSymbol> TargetSemantic { get; }

        public InliningSpecification(
            IntermediateSymbolSemantic<IMethodSymbol> destinationSemantic,
            int inliningId,
            int? parentInliningId,
            ResolvedAspectReference aspectReference, 
            Inliner inliner,
            SyntaxNode replacedRootNode,
            bool useSimpleInlining,
            bool declareReturnVariable,
            string? returnVariableIdentifier,
            string? returnLabelIdentifier,
            IntermediateSymbolSemantic<IMethodSymbol> targetSemantic)
        {
            Invariant.AssertNot( declareReturnVariable && returnVariableIdentifier == null );
            Invariant.Assert( targetSemantic.Kind == IntermediateSymbolSemanticKind.Default );

            this.DestinationSemantic = destinationSemantic;
            this._inliningId = inliningId;
            this._parentInliningId = parentInliningId;
            this.AspectReference = aspectReference;
            this.Inliner = inliner;
            this.UseSimpleInlining = useSimpleInlining;
            this.ReplacedRootNode = replacedRootNode;
            this.ReturnVariableIdentifier = returnVariableIdentifier;
            this.ReturnLabelIdentifier = returnLabelIdentifier;
            this.TargetSemantic = targetSemantic;
        }

        public override string ToString()
        {
            return $"Inline {this.AspectReference.ResolvedSemanticBody} into {this.DestinationSemantic} (id: {this._inliningId})";
        }
    }
}