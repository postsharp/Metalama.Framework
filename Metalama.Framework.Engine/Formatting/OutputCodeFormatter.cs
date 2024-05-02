// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoslynProject = Microsoft.CodeAnalysis.Project;

namespace Metalama.Framework.Engine.Formatting
{
    public static partial class OutputCodeFormatter
    {
        private static async Task<Dictionary<DocumentId, Document>> TransformAllAsync(
            Dictionary<DocumentId, Document> inputDocuments,
            Func<Document, Task<Document>> transformAsync )
        {
            var tasks = inputDocuments.SelectAsMutableList(
                async x =>
                {
                    var newDocument = await transformAsync( x.Value );

                    return new KeyValuePair<DocumentId, Document>( x.Key, newDocument );
                } );

            await Task.WhenAll( tasks );

            return tasks.ToDictionary( t => t.Result.Key, t => t.Result.Value );
        }

        private static async Task<OutputCodeFormatterResult> ApplyToSolutionAsync(
            Solution solution,
            Dictionary<DocumentId, Document> documents,
            CancellationToken cancellationToken )
        {
            var modifiedSolution = solution;

            foreach ( var document in documents )
            {
                modifiedSolution = modifiedSolution
                    .GetDocument( document.Key )
                    .AssertNotNull()
                    .WithSyntaxRoot( (await document.Value.GetSyntaxRootAsync( cancellationToken )).AssertNotNull() )
                    .Project
                    .Solution;
            }

            return new OutputCodeFormatterResult(
                modifiedSolution,
                documents.Keys.ToDictionary( x => x, x => modifiedSolution.GetDocument( x ).AssertNotNull() ) );
        }

        private static async Task<OutputCodeFormatterResult> TransformAllAsync(
            OutputCodeFormatterResult input,
            Func<Document, Task<Document>> transformAsync,
            CancellationToken cancellationToken )
        {
            var documents = await TransformAllAsync( input.Documents, transformAsync );

            return await ApplyToSolutionAsync( input.Solution, documents, cancellationToken );
        }

        public static async ValueTask<Document> FormatAsync(
            Document document,
            IReadOnlyCollection<Diagnostic>? diagnostics = null,
            bool reformatAll = true,
            CancellationToken cancellationToken = default )
        {
            var result = await FormatAsync( document.Project.Solution, [document], diagnostics, reformatAll, cancellationToken );

            return result.Documents.Single().Value;
        }

        public static async ValueTask<OutputCodeFormatterResult> FormatAsync(
            Solution solution,
            IReadOnlyCollection<Document> documents,
            IReadOnlyCollection<Diagnostic>? diagnostics = null,
            bool reformatAll = true,
            CancellationToken cancellationToken = default )
        {
            if ( documents.Count == 0 )
            {
                return new OutputCodeFormatterResult( solution, new Dictionary<DocumentId, Document>() );
            }

            var result = new OutputCodeFormatterResult( documents.First().Project.Solution, documents.ToDictionary( x => x.Id, x => x ) );

            // Add diagnostics as annotations.
            if ( diagnostics is { Count: > 0 } )
            {
                result = await TransformAllAsync(
                    result,
                    async document => document.WithSyntaxRoot(
                        FormattedCodeWriter.AddDiagnosticAnnotations(
                            (await document.GetSyntaxRootAsync( cancellationToken ))!,
                            document.FilePath,
                            diagnostics ) ),
                    cancellationToken );
            }

            // Run custom simplifications.
            result = await TransformAllAsync(
                result,
                async document =>
                {
                    var oldRoot = await document.GetSyntaxRootAsync( cancellationToken );

                    // Try a first time without a semantic mode. 
                    var rewriterWithoutSemanticModel = new CustomSimplifier( null );
                    var newRoot = rewriterWithoutSemanticModel.Visit( oldRoot )!;

                    if ( rewriterWithoutSemanticModel.RequiresSemanticModel )
                    {
                        // If a semantic model is required, do a second run. We assume this is not likely.
                        var semanticModel = await document.GetSemanticModelAsync( cancellationToken );

                        if ( semanticModel == null )
                        {
                            return document;
                        }
                        
                        var rewriterWithSemanticModel = new CustomSimplifier( semanticModel );
                        newRoot = rewriterWithSemanticModel.Visit( oldRoot )!;
                    }

                    return document.WithSyntaxRoot( newRoot );
                },
                cancellationToken );

            // Add imports.
            result = await TransformAllAsync(
                result,
                document => ImportAdder.AddImportsAsync( document, Simplifier.Annotation, cancellationToken: cancellationToken ),
                cancellationToken );

            // Run the simplifier.
            result = await TransformAllAsync( result, document => Simplifier.ReduceAsync( document, Simplifier.Annotation, cancellationToken: cancellationToken ), cancellationToken );

            // Reformat.
            if ( reformatAll )
            {
                result = await TransformAllAsync(
                    result,
                    document => Formatter.FormatAsync( document, cancellationToken: cancellationToken ),
                    cancellationToken );
            }
            else
            {
                result = await TransformAllAsync(
                    result,
                    async document =>
                    {
                        // Figure out which spans need to be reformatted.
                        var classifiedTextSpans = new ClassifiedTextSpanCollection( await document.GetTextAsync( cancellationToken ) );
                        var visitor = new MarkTextSpansVisitor( classifiedTextSpans );
                        visitor.Visit( await document.GetSyntaxRootAsync( cancellationToken ) );
                        classifiedTextSpans.Polish();
                        var generatedSpans = classifiedTextSpans.Where( s => s.Classification == TextSpanClassification.GeneratedCode ).Select( s => s.Span );

                        // Run the reformatter.
                        return await Formatter.FormatAsync(
                            document,
                            generatedSpans,
                            cancellationToken: cancellationToken );
                    },
                    cancellationToken );
            }

            return result;
        }

        internal static async Task<PartialCompilation> FormatAsync( PartialCompilation compilation, CancellationToken cancellationToken = default )
        {
            var (project, syntaxTreeMap) = await CreateProjectFromCompilationAsync( compilation.Compilation, cancellationToken );

            List<SyntaxTreeTransformation> syntaxTreeReplacements = new( compilation.ModifiedSyntaxTrees.Count );
            List<(Document InputDocument, SyntaxTree InputSyntaxTree)> inputDocuments = new( compilation.ModifiedSyntaxTrees.Count );

            foreach ( var modifiedSyntaxTree in compilation.ModifiedSyntaxTrees.Values )
            {
                var syntaxTree = modifiedSyntaxTree.NewTree;

                if ( syntaxTree == null )
                {
                    continue;
                }

                var documentId = syntaxTreeMap[syntaxTree];

                var document = project.GetDocument( documentId )!;

                if ( !document.SupportsSyntaxTree )
                {
                    continue;
                }

                inputDocuments.Add( (document, syntaxTree) );
            }

            var formatted = await FormatAsync( project.Solution, inputDocuments.SelectAsMutableList( d => d.InputDocument ), null, false, cancellationToken );

            foreach ( var document in inputDocuments )
            {
                var newSyntaxTree = (await formatted.Documents[document.InputDocument.Id].GetSyntaxTreeAsync( cancellationToken )).AssertNotNull();

                syntaxTreeReplacements.Add( SyntaxTreeTransformation.ReplaceTree( document.InputSyntaxTree, newSyntaxTree ) );
            }

            return compilation.Update( syntaxTreeReplacements );
        }

        internal static async Task<Compilation> FormatAllAsync( Compilation compilation, CancellationToken cancellationToken = default )
        {
            var (project, syntaxTreeMap) = await CreateProjectFromCompilationAsync( compilation, cancellationToken );

            var documents = compilation.SyntaxTrees.Select( x => project.GetDocument( syntaxTreeMap[x] ) )
                .Where( d => d is { SupportsSyntaxTree: true } )
                .ToReadOnlyList();

            var result = await FormatAsync( project.Solution, documents!, null, true, cancellationToken );

            return (await result.Solution.Projects.Single().GetCompilationAsync( cancellationToken )).AssertNotNull();
        }

        private static async Task<(RoslynProject Project, Dictionary<SyntaxTree, DocumentId> SyntaxTreeMap)> CreateProjectFromCompilationAsync(
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            Dictionary<SyntaxTree, DocumentId> syntaxTreeMap = new();
            var workspace = new AdhocWorkspace();

            var project = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId( compilation.AssemblyName ),
                    VersionStamp.Default,
                    compilation.AssemblyName!,
                    compilation.AssemblyName!,
                    compilation.Language,
                    compilationOptions: compilation.Options,
                    metadataReferences: compilation.References ) );

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                var document = project.AddDocument( syntaxTree.FilePath, await syntaxTree.GetRootAsync( cancellationToken ) );
                project = document.Project;
                syntaxTreeMap.Add( syntaxTree, document.Id );
            }

            return (project, syntaxTreeMap);
        }
    }
}