// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Formatting
{
    public static partial class OutputCodeFormatter
    {
        public static async ValueTask<(Document Document, CompilationUnitSyntax Syntax)> FormatAsync(
            Document document,
            IEnumerable<Diagnostic>? diagnostics = null,
            bool reformatAll = true,
            CancellationToken cancellationToken = default )
        {
            if ( diagnostics != null )
            {
                document = document.WithSyntaxRoot(
                    FormattedCodeWriter.AddDiagnosticAnnotations( (await document.GetSyntaxRootAsync( cancellationToken ))!, document.FilePath, diagnostics ) );
            }

            var documentWithImports = await ImportAdder.AddImportsAsync( document, Simplifier.Annotation, cancellationToken: cancellationToken );
            var simplifiedDocument = await Simplifier.ReduceAsync( documentWithImports, cancellationToken: cancellationToken );

            Document formattedDocument;

            if ( reformatAll )
            {
                formattedDocument = await Formatter.FormatAsync( simplifiedDocument, cancellationToken: cancellationToken );
            }
            else
            {
                var classifiedTextSpans = new ClassifiedTextSpanCollection( await simplifiedDocument.GetTextAsync( cancellationToken ) );
                var visitor = new MarkTextSpansVisitor( classifiedTextSpans );
                visitor.Visit( await simplifiedDocument.GetSyntaxRootAsync( cancellationToken ) );
                classifiedTextSpans.Polish();
                var generatedSpans = classifiedTextSpans.Where( s => s.Classification == TextSpanClassification.GeneratedCode ).Select( s => s.Span );

                formattedDocument = await Formatter.FormatAsync(
                    simplifiedDocument,
                    generatedSpans,
                    cancellationToken: cancellationToken );
            }

            var formattedSyntax = (CompilationUnitSyntax) (await formattedDocument.GetSyntaxRootAsync( cancellationToken ))!;

            var finalSyntax = EndOfLineHelper.NormalizeEndOfLineStyle( formattedSyntax );

            var finalDocument = formattedDocument.WithSyntaxRoot( finalSyntax );

            return (finalDocument, finalSyntax);
        }

        public static async Task<PartialCompilation> FormatAsync( PartialCompilation compilation, CancellationToken cancellationToken = default )
        {
            var (project, syntaxTreeMap) = await CreateProjectFromCompilationAsync( compilation.Compilation, cancellationToken );

            List<SyntaxTreeTransformation> syntaxTreeReplacements = new( compilation.ModifiedSyntaxTrees.Count );

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

                var formatted = await FormatAsync( document, null, false, cancellationToken );

                syntaxTreeReplacements.Add(
                    SyntaxTreeTransformation.ReplaceTree( syntaxTree, syntaxTree.WithRootAndOptions( formatted.Syntax, syntaxTree.Options ) ) );
            }

            return compilation.Update( syntaxTreeReplacements );
        }

        public static Compilation FormatAll( Compilation compilation, CancellationToken cancellationToken = default )
            => TaskHelper.RunAndWait( () => FormatAllAsync( compilation, cancellationToken ), cancellationToken );

        private static async Task<Compilation> FormatAllAsync( Compilation compilation, CancellationToken cancellationToken = default )
        {
            var formattedCompilation = compilation;
            var (project, syntaxTreeMap) = await CreateProjectFromCompilationAsync( compilation, cancellationToken );

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                var documentId = syntaxTreeMap[syntaxTree];

                var document = project.GetDocument( documentId )!;

                if ( !document.SupportsSyntaxTree )
                {
                    continue;
                }

                var formatted = await FormatAsync( document, null, true, cancellationToken );

                formattedCompilation = formattedCompilation.ReplaceSyntaxTree(
                    syntaxTree,
                    syntaxTree.WithRootAndOptions( formatted.Syntax, syntaxTree.Options ) );
            }

            return formattedCompilation;
        }

        private static async Task<(Microsoft.CodeAnalysis.Project Project, Dictionary<SyntaxTree, DocumentId> SyntaxTreeMap)> CreateProjectFromCompilationAsync(
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

        // HACK: We cannot format the output if the current AppDomain does not contain the workspace assemblies.
        // Code formatting is used by TryMetalama only now. Somehow TryMetalama also builds through the command line for some
        // initialization, which triggers an error because we don't ship all necessary assemblies.

        public static bool CanFormat => AppDomainUtility.HasAnyLoadedAssembly( a => a.GetName().Name == "Microsoft.CodeAnalysis.Workspaces" );
    }
}