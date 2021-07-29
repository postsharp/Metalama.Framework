// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Observers;
using Caravela.Framework.Impl.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Templating
{
    internal class TemplateCompiler
    {
        public const string TemplateMethodSuffix = "_Template";

        private readonly IServiceProvider _serviceProvider;
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap = new();
        private readonly ITemplateCompilerObserver? _observer;
        private readonly SerializableTypes _serializableTypes;

        public TemplateCompiler( IServiceProvider serviceProvider, Compilation runTimeCompilation )
        {
            this._serviceProvider = serviceProvider;
            var syntaxSerializationService = serviceProvider.GetService<SyntaxSerializationService>();
            this._serializableTypes = syntaxSerializationService.GetSerializableTypes( ReflectionMapper.GetInstance( runTimeCompilation ) );

            this._observer = serviceProvider.GetOptionalService<ITemplateCompilerObserver>();
        }

        public ILocationAnnotationMapBuilder LocationAnnotationMap => this._syntaxTreeAnnotationMap;

        public bool TryAnnotate(
            SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot )
        {
            SyntaxNode currentSyntaxRoot;

            void FixupTreeForDiagnostics()
            {
                // Put the annotated node back into the original tree, so that diagnostics have correct locations.
                var markerAnnotation = new SyntaxAnnotation();

                var annotatedTree = sourceSyntaxRoot.SyntaxTree.GetRoot()
                    .ReplaceNode(
                        sourceSyntaxRoot,
                        currentSyntaxRoot.WithAdditionalAnnotations( markerAnnotation ) );

                currentSyntaxRoot = annotatedTree.GetAnnotatedNodes( markerAnnotation ).Single();
            }

            // Annotate the syntax tree with symbols.
            currentSyntaxRoot = this._syntaxTreeAnnotationMap.AnnotateTemplate( sourceSyntaxRoot, semanticModel );

            FixupTreeForDiagnostics();

            annotatedSyntaxRoot = currentSyntaxRoot;

            // Annotate the syntax tree with info about build- and run-time nodes,
            var annotatorRewriter = new TemplateAnnotator(
                (CSharpCompilation) semanticModel.Compilation,
                this._syntaxTreeAnnotationMap,
                diagnostics,
                this._serviceProvider,
                this._serializableTypes,
                cancellationToken );

            annotatedSyntaxRoot = annotatorRewriter.Visit( annotatedSyntaxRoot )!;

            this._observer?.OnAnnotatedSyntaxNode( sourceSyntaxRoot, annotatedSyntaxRoot );

            // Stop if we have any error.
            if ( !annotatorRewriter.Success )
            {
                return false;
            }

            return true;
        }

        public bool TryCompile(
            string templateName,
            Compilation compileTimeCompilation,
            SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot,
            [NotNullWhen( true )] out SyntaxNode? transformedSyntaxRoot )
        {
            if ( !this.TryAnnotate( sourceSyntaxRoot, semanticModel, diagnostics, cancellationToken, out annotatedSyntaxRoot ) )
            {
                transformedSyntaxRoot = null;

                return false;
            }

            // TODO: Reporting diagnostics here can result in duplicate reports because there may be several templates in one syntax tree.
            var sourceDiagnostics = semanticModel.GetDiagnostics( sourceSyntaxRoot.Span, cancellationToken );

            if ( sourceDiagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                // Don't continue with errors in source code (note however that we do the annotation with errors because of real-time syntax highlighting).
                annotatedSyntaxRoot = null;
                transformedSyntaxRoot = null;
                diagnostics.Report( sourceDiagnostics );

                return false;
            }

            // Compile the syntax tree.
            var templateCompilerRewriter = new TemplateCompilerRewriter(
                templateName,
                semanticModel.Compilation,
                compileTimeCompilation,
                this._syntaxTreeAnnotationMap,
                diagnostics,
                this._serviceProvider,
                this._serializableTypes,
                cancellationToken );

            transformedSyntaxRoot = templateCompilerRewriter.Visit( annotatedSyntaxRoot );

            return transformedSyntaxRoot != null && templateCompilerRewriter.Success;
        }
    }
}