﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class ResolvedAspectReference
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
            => (this.ResolvedSemantic, this.TargetKind) switch
            {
                ({ Symbol: IMethodSymbol method }, AspectReferenceTargetKind.Self) =>
                    method.ToSemantic( this.ResolvedSemantic.Kind ),
                ({ Symbol: IPropertySymbol property }, AspectReferenceTargetKind.PropertyGetAccessor) =>
                    property.GetMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                ({ Symbol: IPropertySymbol { SetMethod: null } property }, AspectReferenceTargetKind.PropertySetAccessor) =>
                    property.GetMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                ({ Symbol: IPropertySymbol property }, AspectReferenceTargetKind.PropertySetAccessor) =>
                    property.SetMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                ({ Symbol: IEventSymbol @event }, AspectReferenceTargetKind.EventAddAccessor) =>
                    @event.AddMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                ({ Symbol: IEventSymbol @event }, AspectReferenceTargetKind.EventRemoveAccessor) =>
                    @event.RemoveMethod.AssertNotNull().ToSemantic( this.ResolvedSemantic.Kind ),
                _ => throw new AssertionFailedException( $"{this} does not point to a semantic with a body." )
            };

        public bool HasResolvedSemanticBody
            => (this.ResolvedSemantic, this.TargetKind) switch
            {
                ({ Symbol: IMethodSymbol }, AspectReferenceTargetKind.Self) => true,
                ({ Symbol: IPropertySymbol }, AspectReferenceTargetKind.PropertyGetAccessor) => true,
                ({ Symbol: IPropertySymbol }, AspectReferenceTargetKind.PropertySetAccessor) => true,
                ({ Symbol: IEventSymbol }, AspectReferenceTargetKind.EventAddAccessor) => true,
                ({ Symbol: IEventSymbol }, AspectReferenceTargetKind.EventRemoveAccessor) => true,
                ({ Symbol: IEventSymbol }, AspectReferenceTargetKind.EventRaiseAccessor) => false,
                ({ Symbol: IFieldSymbol }, AspectReferenceTargetKind.PropertyGetAccessor) => false,
                ({ Symbol: IFieldSymbol }, AspectReferenceTargetKind.PropertySetAccessor) => false,
                _ => throw new AssertionFailedException( $"{this} is not expected." )
            };
        
#if DEBUG
        
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local

        /// <summary>
        /// Gets the annotated node. This is the node that originally had the annotation.
        /// </summary>
#pragma warning disable IDE0052
        private SyntaxNode AnnotatedNode { get; }
#pragma warning restore IDE0052
        
#endif
        
        /// <summary>
        /// Gets the root node. This is the node that needs to be replaced by the linker.
        /// </summary>
        public SyntaxNode RootNode { get; }

        /// <summary>
        /// Gets the annotated expression. This is for convenience in inliners which always work with expressions.
        /// </summary>
        public ExpressionSyntax RootExpression
            => this.RootNode as ExpressionSyntax ?? throw new AssertionFailedException( $"Root node {this.RootNode.Kind()} is not an expression." );

        /// <summary>
        /// Gets the symbol source node. This node is the source of the symbol that is referenced.
        /// </summary>
        public SyntaxNode SymbolSourceNode { get; }

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
            SyntaxNode annotatedNode,
            SyntaxNode rootNode,
            SyntaxNode symbolSourceNode,
            AspectReferenceTargetKind targetKind,
            bool isInlineable )
        {
            Invariant.AssertNot( containingSemantic.Kind != IntermediateSymbolSemanticKind.Final && symbolSourceNode is not ExpressionSyntax );

            Invariant.AssertNot(
                resolvedSemantic.Symbol is IMethodSymbol
                {
                    MethodKind: not MethodKind.Ordinary and not MethodKind.ExplicitInterfaceImplementation and not MethodKind.Destructor
                    and not MethodKind.UserDefinedOperator and not MethodKind.Conversion
                } );

            this.ContainingSemantic = containingSemantic;
            this.OriginalSymbol = originalSymbol;
            this.ResolvedSemantic = resolvedSemantic;
            this.AnnotatedNode = annotatedNode;
            this.RootNode = rootNode;
            this.SymbolSourceNode = symbolSourceNode;
            this.IsInlineable = isInlineable;
            this.TargetKind = targetKind;
        }

        public override string ToString()
        {
            return $"{this.ContainingSemantic} ({(this.RootNode is ExpressionSyntax ? this.RootNode : "not expression")}) -> {this.ResolvedSemantic}";
        }
    }
}