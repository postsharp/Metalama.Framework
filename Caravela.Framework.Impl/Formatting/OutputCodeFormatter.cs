// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.Formatting
{
    public sealed class OutputCodeFormatter
    {
        /// <summary>
        /// Annotation used to mark locals and 'return;' statement that may be redundant. Currently we are not doing anything with them,
        /// but we could.
        /// </summary>
        public static readonly SyntaxAnnotation PossibleRedundantAnnotation = new( "Caravela_PossibleRedundant" );

        public static async ValueTask<CompilationUnitSyntax> FormatAsync(
            Document document,
            bool reformatAll = true,
            CancellationToken cancellationToken = default )
        {
            var documentWithImports = await ImportAdder.AddImportsAsync( document, Simplifier.Annotation, cancellationToken: cancellationToken );
            var simplifiedDocument = await Simplifier.ReduceAsync( documentWithImports, cancellationToken: cancellationToken );

            var outputSyntaxRoot = (CompilationUnitSyntax) (await simplifiedDocument.GetSyntaxRootAsync( cancellationToken ))!;

            if ( reformatAll )
            {
                outputSyntaxRoot = (CompilationUnitSyntax) Formatter.Format( outputSyntaxRoot, document.Project.Solution.Workspace );
            }
            else
            {
                outputSyntaxRoot = (CompilationUnitSyntax) Formatter.Format(
                    outputSyntaxRoot,
                    AspectPipelineAnnotations.GeneratedCode,
                    document.Project.Solution.Workspace );
            }

            return outputSyntaxRoot;
        }

        public static async Task<PartialCompilation> FormatAsync( PartialCompilation compilation, CancellationToken cancellationToken = default )
        {
            var (project, syntaxTreeMap) = await CreateProjectFromCompilation( compilation.Compilation, cancellationToken );

            List<ModifiedSyntaxTree> syntaxTreeReplacements = new( compilation.ModifiedSyntaxTrees.Count );

            foreach ( var modifiedSyntaxTree in compilation.ModifiedSyntaxTrees.Values )
            {
                var syntaxTree = modifiedSyntaxTree.NewTree;
                var documentId = syntaxTreeMap[syntaxTree];

                var document = project.GetDocument( documentId )!;

                if ( !document.SupportsSyntaxTree )
                {
                    continue;
                }

                var formattedSyntaxRoot = await FormatAsync( document, false, cancellationToken );

                syntaxTreeReplacements.Add( new ModifiedSyntaxTree( syntaxTree.WithRootAndOptions( formattedSyntaxRoot, syntaxTree.Options ), syntaxTree ) );
            }

            return compilation.UpdateSyntaxTrees( syntaxTreeReplacements, Array.Empty<SyntaxTree>() );
        }

        private static async Task<(Project Project, Dictionary<SyntaxTree, DocumentId> SyntaxTreeMap)> CreateProjectFromCompilation(
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            Dictionary<SyntaxTree, DocumentId> syntaxTreeMap = new();
            AdhocWorkspace workspace = new();

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