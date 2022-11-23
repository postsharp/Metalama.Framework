// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Templating
{
    internal class TemplateCompiler
    {
        private readonly ProjectServiceProvider _serviceProvider;
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
        private readonly ITemplateCompilerObserver? _observer;
        private readonly SerializableTypes _serializableTypes;
        private readonly ILogger _logger;

        public TemplateCompiler( ProjectServiceProvider serviceProvider, Compilation runTimeCompilation )
        {
            this._syntaxTreeAnnotationMap = new SyntaxTreeAnnotationMap( runTimeCompilation );
            this._serviceProvider = serviceProvider;
            this._logger = serviceProvider.GetLoggerFactory().CompileTime();

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
            out SyntaxNode annotatedSyntaxRoot,
            out RoslynApiVersion usedApiVersion )
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

            // Verify the language version of the template. We now support only C# 10.0.
            var versionVerifier = new RoslynVersionSyntaxVerifier( diagnostics, RoslynApiVersion.V4_0_1, "10.0" );
            versionVerifier.Visit( sourceSyntaxRoot );
            usedApiVersion = versionVerifier.MaximalUsedVersion;

            // Annotate the syntax tree with symbols.
            if ( !this._syntaxTreeAnnotationMap.TryAnnotateTemplate( sourceSyntaxRoot, semanticModel, diagnostics, out currentSyntaxRoot ) )
            {
                annotatedSyntaxRoot = currentSyntaxRoot;

                return false;
            }

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
            TemplateCompilerSemantics templateSyntaxKind,
            SemanticModel semanticModel,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot,
            [NotNullWhen( true )] out SyntaxNode? transformedSyntaxRoot )
        {
            if ( !this.TryAnnotate( sourceSyntaxRoot, semanticModel, diagnostics, cancellationToken, out annotatedSyntaxRoot, out var usedApiVersion ) )
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

                if ( this._logger.Trace != null )
                {
                    this._logger.Trace.Log(
                        $"Cannot create a compile-time assembly for '{semanticModel.SyntaxTree.FilePath}' because there are diagnostics in the source code:" );

                    foreach ( var error in errors )
                    {
                        this._logger.Trace.Log( "    " + error );
                    }

                    this._logger.Trace.Log( $"  Compilation id: {DebuggingHelper.GetObjectId( semanticModel.Compilation )}" );

                    this._logger.Trace.Log( "Syntax trees:" );

                    foreach ( var syntaxTree in semanticModel.Compilation.SyntaxTrees )
                    {
                        this._logger.Trace.Log( "   " + syntaxTree.FilePath );
                    }

                    this._logger.Trace.Log( "Compilation references: " );

                    foreach ( var reference in semanticModel.Compilation.References )
                    {
                        switch ( reference )
                        {
                            case PortableExecutableReference:
                                // Skipped because there are too many of them and they are never wrong.
                                break;

                            case CompilationReference compilation:
                                this._logger.Trace.Log( $"Project: {compilation.Display} ({compilation.Compilation.SyntaxTrees.Count()} syntax tree(s))" );

                                break;

                            default:
                                this._logger.Trace.Log( "Other: " + reference );

                                break;
                        }
                    }
                }

                return false;
            }

            // ReSharper restore PossibleMultipleEnumeration

            var compileTimeCompilationServices = this._serviceProvider.GetRequiredService<CompilationServicesFactory>().GetInstance( compileTimeCompilation );
            
            // Compile the syntax tree.
            var templateCompilerRewriter = new TemplateCompilerRewriter(
                templateName,
                templateSyntaxKind,
                semanticModel.Compilation,
                compileTimeCompilation,
                this._syntaxTreeAnnotationMap,
                diagnostics,
                compileTimeCompilationServices,
                this._serializableTypes,
                usedApiVersion,
                cancellationToken );

            transformedSyntaxRoot = templateCompilerRewriter.Visit( annotatedSyntaxRoot );

            return transformedSyntaxRoot != null && templateCompilerRewriter.Success;
        }
    }
}