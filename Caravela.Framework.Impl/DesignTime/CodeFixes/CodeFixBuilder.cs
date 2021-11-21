// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.CodeFixes;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    internal partial class CodeFixBuilder : ICodeFixBuilder
    {
        private readonly CodeFixContext _context;
        private readonly HashSet<DocumentId> _changedDocuments = new();
        private Solution _solution;
        private bool _hasChange;

        public CodeFixBuilder( CodeFixContext context, CancellationToken cancellationToken )
        {
            this._context = context;
            this._solution = this._context.OriginalDocument.Project.Solution;
            this.CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }

        public async Task<Solution> GetResultingSolutionAsync()
        {
            var formattedSolution = this._solution;

            foreach ( var changedDocumentId in this._changedDocuments )
            {
                var formattingResult = await OutputCodeFormatter.FormatToDocumentAsync(
                    formattedSolution.GetDocument( changedDocumentId )!,
                    null,
                    false,
                    this.CancellationToken );

                formattedSolution = formattingResult.Document.Project.Solution;
            }

            return formattedSolution;
        }

        public async Task<bool> AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute )
        {
            this.CheckChange();

            this.CancellationToken.ThrowIfCancellationRequested();

            var compilation = await this._solution.GetProject( this._context.OriginalDocument.Project.Id )!.GetCompilationAsync( this.CancellationToken );

            if ( compilation == null )
            {
                // TODO: Error. Log.
                return false;
            }

            var targetSymbol = targetDeclaration.ToRef().GetSymbol( compilation! );

            if ( targetSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(targetDeclaration), "The declaration is not declared in source." );
            }

            var originalNode = targetDeclaration.GetPrimaryDeclaration().AssertNotNull();

            if ( originalNode is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDeclaration } )
            {
                originalNode = variableDeclaration.Parent!;
            }
            
            var originalRoot = await originalNode.SyntaxTree.GetRootAsync( this.CancellationToken );
            var originalDocument = this._solution.GetDocument( originalNode.SyntaxTree ).AssertNotNull();

            var generationContext = SyntaxGenerationContext.Create( this._context.ServiceProvider, compilation, originalNode );
            var transformedNode = generationContext.SyntaxGenerator.AddAttribute( originalNode, attribute, generationContext.ReflectionMapper );

            var transformedRoot = originalRoot.ReplaceNode( originalNode, transformedNode );

            this._solution = this._solution.WithDocumentSyntaxRoot( originalDocument.Id, transformedRoot );
            this._changedDocuments.Add( originalDocument.Id );

            return true;
        }

        public async Task<bool> RemoveAttributeAsync( IDeclaration targetDeclaration, INamedType attributeType )
        {
            this.CancellationToken.ThrowIfCancellationRequested();

            var compilation = await this._solution.GetProject( this._context.OriginalDocument.Project.Id )!.GetCompilationAsync( this.CancellationToken );

            if ( compilation == null )
            {
                // TODO: Error. Log.
                return false;
            }

            var attributeTypeSymbol = (ITypeSymbol) attributeType.ToRef().GetSymbol( compilation! );
            
            var targetSymbol = targetDeclaration.ToRef().GetSymbol( compilation! );

            if ( targetSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(targetDeclaration), "The declaration is not declared in source." );
            }

            // We need to process all syntaxes that define this symbol.
            foreach ( var syntaxReferenceGroup in targetSymbol.DeclaringSyntaxReferences.GroupBy( r => r.SyntaxTree ) )
            {
                var syntaxTree = syntaxReferenceGroup.Key;
                var originalRoot = await syntaxTree.GetRootAsync( this.CancellationToken );
                
                var originalDocument = this._solution.GetDocument( syntaxTree ).AssertNotNull();

                var rewriter = new RemoveAttributeRewriter( compilation.GetSemanticModel( syntaxTree ), attributeTypeSymbol );

                var transformedRoot = originalRoot;
                var syntaxNodes = new List<SyntaxNode>();
                
                foreach ( var syntaxReference in syntaxReferenceGroup )
                {
                    var originalNode = await syntaxReference.GetSyntaxAsync( this.CancellationToken );
                    syntaxNodes.Add( originalNode );
                } 

                transformedRoot = transformedRoot.ReplaceNodes( syntaxNodes, ( node, _ ) => rewriter.Visit( node ) );

                this._solution = this._solution.WithDocumentSyntaxRoot( originalDocument.Id, transformedRoot );
                this._changedDocuments.Add( originalDocument.Id );
            }
            
            return true;
        }

        public async Task<bool> ApplyAspectAsync<TTarget>( TTarget targetDeclaration, IAspect<TTarget> aspect )
            where TTarget : class, IDeclaration
        {
            var project = this._solution.GetProject( this._context.OriginalDocument.Project.Id )!;
            var compilation = await project.GetCompilationAsync( this.CancellationToken );

            if ( compilation == null )
            {
                // TODO: Error. Log.
                return false;
            }

            var targetSymbol = targetDeclaration.ToRef().GetSymbol( compilation! );

            if ( targetSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(targetDeclaration), "The declaration is not declared in source." );
            }

            var aspectClass = (AspectClass) this._context.PipelineConfiguration.AspectClasses.Single( c => c.Type == aspect.GetType() );

            if ( !this._context.PipelineFactory.TryApplyAspectToCode(
                this._context.ProjectOptions,
                aspectClass,
                aspect,
                compilation,
                targetSymbol,
                this.CancellationToken,
                out var resultCompilation,
                out var diagnostics ) )
            {
                this._solution = await CodeFixHelper.ReportDiagnosticsAsCommentsAsync(
                    targetSymbol,
                    this._context.OriginalDocument,
                    diagnostics,
                    this.CancellationToken );

                return false;
            }

            this._solution = await CodeFixHelper.ApplyChangesAsync( resultCompilation, project, this.CancellationToken );

            return true;
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