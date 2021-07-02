// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.Formatting
{
    public sealed class OutputCodeFormatter
    {
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
    }
}