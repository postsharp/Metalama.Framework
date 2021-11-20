// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.CodeFixes;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Formatting;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    internal class CodeFixBuilder : ICodeFixBuilder
    {
        private readonly CodeFixContext _context;
        private CompilationModel? _compilation;
        private Solution _solution;
        private bool _hasChange;
        private HashSet<DocumentId> _changedDocuments = new();

        public CodeFixBuilder( CodeFixContext context, CancellationToken cancellationToken )
        {
            this._context = context;
            this._solution = this._context.OriginalDocument.Project.Solution;
            this._compilation = this._context.OriginalCompilationModel;
            this.CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }

        public async Task<Solution> GetResultingSolutionAsync()
        {
            var formattedSolution = this._solution;

            foreach ( var modifiedDocumentId in this._changedDocuments )
            {
                var formattingResult = await OutputCodeFormatter.FormatToDocumentAsync(
                    formattedSolution.GetDocument( modifiedDocumentId )!,
                    null,
                    false,
                    this.CancellationToken );

                formattedSolution = formattingResult.Document.Project.Solution;
            }

            return formattedSolution;
        }

        public async Task AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute )
        {
            this.CheckChange();

            this.CancellationToken.ThrowIfCancellationRequested();

            var resolvedDeclaration = this._compilation!.Factory.GetDeclaration( targetDeclaration );
            var targetSymbol = resolvedDeclaration.GetSymbol();

            if ( targetSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(targetDeclaration), "The declaration is not declared in source." );
            }

            var originalNode = resolvedDeclaration.GetPrimaryDeclaration().AssertNotNull();
            var originalRoot = await originalNode.SyntaxTree.GetRootAsync( this.CancellationToken );
            var originalDocument = this._solution.GetDocument( originalNode.SyntaxTree ).AssertNotNull();

            var generationContext = SyntaxGenerationContext.Create( this._context.ServiceProvider, this._compilation.RoslynCompilation, originalNode );
            var transformedNode = generationContext.SyntaxGenerator.AddAttribute( originalNode, attribute, generationContext.ReflectionMapper );

            var transformedRoot = originalRoot.ReplaceNode( originalNode, transformedNode );

            this._solution = this._solution.WithDocumentSyntaxRoot( originalDocument.Id, transformedRoot );
            this._changedDocuments.Add( originalDocument.Id );

            this._compilation = null;
        }

        public Task RemoveAttributeAsync( IDeclaration declaration, INamedType attributeType ) => throw new NotImplementedException();

        private void CheckChange()
        {
            if ( this._hasChange )
            {
                throw new InvalidOperationException( "Code actions must currently be composed of a single transformation." );
            }

            this._hasChange = true;
        }

        public Task ApplyLiveTemplateAsync<TTarget>( TTarget declaration, ILiveTemplate<TTarget> liveTemplate )
            where TTarget : class, IDeclaration
        {
            this.CheckChange();

            this.CancellationToken.ThrowIfCancellationRequested();

            throw new NotImplementedException();
        }
    }
}