// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// The implementation of <see cref="ICodeFixBuilder"/>, passed to user code.
    /// </summary>
    internal partial class CodeFixBuilder : ICodeFixBuilder
    {
        private readonly CodeFixContext _context;
        private readonly HashSet<string> _changedSyntaxTrees = new();
        private readonly DiagnosticList _diagnostics = new();
        private PartialCompilation _compilation;
        private bool _hasChange;

        public CodeFixBuilder( CodeFixContext context, CancellationToken cancellationToken )
        {
            this._context = context;
            this._compilation = this._context.OriginalCompilation;
            this.CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }

        public CodeActionResult ToCodeActionResult() => new( this._changedSyntaxTrees.Select( x => this._compilation.SyntaxTrees[x] ).ToImmutableArray() );

        public async Task<bool> AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute )
        {
            this.CheckChange();

            this.CancellationToken.ThrowIfCancellationRequested();

            var compilation = this._compilation.Compilation;

            var targetSymbol = targetDeclaration.ToRef().GetSymbol( compilation );

            if ( targetSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(targetDeclaration), "The declaration is not declared in source." );
            }

            var originalNode = targetDeclaration.GetPrimaryDeclaration().AssertNotNull();

            if ( originalNode is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDeclaration } )
            {
                originalNode = variableDeclaration.Parent!;
            }

            var originalTree = originalNode.SyntaxTree;
            var originalRoot = await originalTree.GetRootAsync( this.CancellationToken );

            var generationContext = SyntaxGenerationContext.Create( this._context.ServiceProvider, compilation, originalNode );
            var transformedNode = generationContext.SyntaxGenerator.AddAttribute( originalNode, attribute, generationContext.ReflectionMapper );

            var transformedRoot = originalRoot.ReplaceNode( originalNode, transformedNode );

            this.UpdateTree( transformedRoot, originalTree );

            return true;
        }

        private void UpdateTree( SyntaxTree transformedTree, SyntaxTree originalTree )
        {
            this._compilation = this._compilation.Update( new[] { new SyntaxTreeModification( transformedTree, originalTree ) } );
            this._changedSyntaxTrees.Add( originalTree.FilePath );
        }

        private void UpdateTree( SyntaxNode transformedRoot, SyntaxTree originalTree )
        {
            var transformedTree = CSharpSyntaxTree.Create(
                (CSharpSyntaxNode) transformedRoot,
                (CSharpParseOptions?) originalTree.Options,
                originalTree.FilePath,
                originalTree.Encoding );

            this.UpdateTree( transformedTree, originalTree );
        }

        public async Task<bool> RemoveAttributesAsync( IDeclaration targetDeclaration, INamedType attributeType )
        {
            this.CancellationToken.ThrowIfCancellationRequested();

            var compilation = this._compilation.Compilation;

            var attributeTypeSymbol = (ITypeSymbol?) attributeType.GetSymbol( compilation );

            if ( attributeTypeSymbol == null )
            {
                return false;
            }

            var targetSymbol = targetDeclaration.GetSymbol( compilation );

            if ( targetSymbol == null )
            {
                return false;
            }

            // We need to process all syntaxes that define this symbol.
            foreach ( var syntaxReferenceGroup in targetSymbol.DeclaringSyntaxReferences.GroupBy( r => r.SyntaxTree ) )
            {
                var originalTree = syntaxReferenceGroup.Key;
                var originalRoot = await originalTree.GetRootAsync( this.CancellationToken );

                var rewriter = new RemoveAttributeRewriter( compilation.GetSemanticModel( originalTree ), attributeTypeSymbol );

                var transformedRoot = originalRoot;
                var syntaxNodes = new List<SyntaxNode>();

                foreach ( var syntaxReference in syntaxReferenceGroup )
                {
                    var originalNode = await syntaxReference.GetSyntaxAsync( this.CancellationToken );
                    syntaxNodes.Add( originalNode );
                }

                transformedRoot = transformedRoot.ReplaceNodes( syntaxNodes, ( node, _ ) => rewriter.Visit( node ) );

                this.UpdateTree( transformedRoot, originalTree );
            }

            return true;
        }

        public Task<bool> RemoveAttributesAsync( IDeclaration targetDeclaration, Type attributeType )
            => this.RemoveAttributesAsync( targetDeclaration, (INamedType) targetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( attributeType ) );

        public async Task<bool> ApplyAspectAsync<TTarget>( TTarget targetDeclaration, IAspect<TTarget> aspect )
            where TTarget : class, IDeclaration
        {
            var compilation = this._compilation.Compilation;

            var targetSymbol = targetDeclaration.ToRef().GetSymbol( compilation );

            if ( targetSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(targetDeclaration), "The declaration is not declared in source." );
            }

            var aspectClass = (AspectClass) this._context.PipelineConfiguration.BoundAspectClasses.Single<IBoundAspectClass>( c => c.Type == aspect.GetType() );

            if ( !LiveTemplateAspectPipeline.TryExecute(
                    this._context.ServiceProvider,
                    this._context.PipelineConfiguration.Domain,
                    this._context.PipelineConfiguration,
                    _ => aspectClass,
                    PartialCompilation.CreatePartial( compilation, targetSymbol.GetPrimaryDeclaration()!.SyntaxTree ),
                    targetSymbol,
                    this._diagnostics,
                    CancellationToken.None,
                    out var outputCompilation ) )
            {
                return false;
            }
            else
            {
                this._compilation = this._compilation.Update(
                    outputCompilation.ModifiedSyntaxTrees.Values.Where( x => x.OldTree != null ).ToList(),
                    outputCompilation.ModifiedSyntaxTrees.Values.Where( x => x.OldTree == null ).Select( x => x.NewTree ).ToList() );

                foreach ( var modifiedPath in outputCompilation.ModifiedSyntaxTrees.Keys )
                {
                    this._changedSyntaxTrees.Add( modifiedPath );
                }

                return true;
            }
        }

        private void CheckChange()
        {
            if ( this._hasChange )
            {
                throw new InvalidOperationException( "Code actions must currently be composed of a single transformation." );
            }

            this._hasChange = true;
        }
    }
}