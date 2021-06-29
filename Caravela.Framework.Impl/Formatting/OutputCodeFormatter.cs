// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.Formatting
{
    public sealed class OutputCodeFormatter
    {
        public static async ValueTask<CompilationUnitSyntax> FormatAsync( Document document, CancellationToken cancellationToken = default )
        {
            var simplifiedDocument = await Simplifier.ReduceAsync( document, cancellationToken: cancellationToken );
            var simplifiedSyntaxRoot = (await simplifiedDocument.GetSyntaxRootAsync( cancellationToken ))!;

            return (CompilationUnitSyntax) Formatter.Format( simplifiedSyntaxRoot, document.Project.Solution.Workspace );
        }
    }
}