// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Templating
{
    internal class TemplateCompiler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
        private readonly ITemplateCompilerObserver? _observer;
        private readonly SerializableTypes _serializableTypes;

        public TemplateCompiler( IServiceProvider serviceProvider, Compilation runTimeCompilation )
        {
            this._syntaxTreeAnnotationMap = new SyntaxTreeAnnotationMap( runTimeCompilation );
            this._serviceProvider = serviceProvider;
            var syntaxSerializationService = serviceProvider.GetRequiredService<SyntaxSerializationService>();
            this._serializableTypes = syntaxSerializationService.GetSerializableTypes( runTimeCompilation );

            this._observer = serviceProvider.GetService<ITemplateCompilerObserver>();
        }

        public ILocationAnnotationMapBuilder LocationAnnotationMap => this._syntaxTreeAnnotationMap;

        public bool TryAnnotate(
            SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            out SyntaxNode annotatedSyntaxRoot )
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
            TemplateSyntaxKind templateSyntaxKind,
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

            var sourceDiagnostics = semanticModel.GetDiagnostics( sourceSyntaxRoot.Span, cancellationToken );

            var errors = sourceDiagnostics.Where( d => d.Severity == DiagnosticSeverity.Error );

            // ReSharper disable PossibleMultipleEnumeration
            if ( errors.Any() )
            {
                // Don't continue with errors in source code (note however that we do the annotation with errors because of real-time syntax highlighting).
                // Diagnostics don't need to be reported because they would be reported by the compiler anyway.
                // Note that it's ok to annotate a template that has errors. This is used by syntax highlighting. 

                annotatedSyntaxRoot = null;
                transformedSyntaxRoot = null;

                if ( Logger.Instance != null )
                {
                    Logger.Instance.Write(
                        $"Cannot create a compile-time assembly for '{semanticModel.SyntaxTree.FilePath}' because there are diagnostics in the source code:" );

                    foreach ( var error in errors )
                    {
                        Logger.Instance.Write( "    " + error );
                    }

                    Logger.Instance.Write( $"  Compilation id: {DebuggingHelper.GetObjectId( semanticModel.Compilation )}" );

                    Logger.Instance.Write( "Syntax trees:" );

                    foreach ( var syntaxTree in semanticModel.Compilation.SyntaxTrees )
                    {
                        Logger.Instance.Write( "   " + syntaxTree.FilePath );
                    }

                    Logger.Instance.Write( "Compilation references: " );

                    foreach ( var reference in semanticModel.Compilation.References )
                    {
                        switch ( reference )
                        {
                            case PortableExecutableReference:
                                // Skipped because there are too many of them and they are never wrong.
                                break;

                            case CompilationReference compilation:
                                Logger.Instance.Write( $"Project: {compilation.Display} ({compilation.Compilation.SyntaxTrees.Count()} syntax tree(s))" );

                                break;

                            default:
                                Logger.Instance.Write( "Other: " + reference );

                                break;
                        }
                    }
                }

                return false;
            }

            // ReSharper restore PossibleMultipleEnumeration

            // Compile the syntax tree.
            var templateCompilerRewriter = new TemplateCompilerRewriter(
                templateName,
                templateSyntaxKind,
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