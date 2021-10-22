// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Exposes a method <see cref="GetClassifiedTextSpans"/> that returns a set of <see cref="IClassifiedTextSpan"/>
    /// saying how a <see cref="SyntaxTree"/> must be colored in the editor.
    /// </summary>
    [Guid( "0a2a7b74-a701-468b-a000-3f1bbd7eda4d" )]
    [ComImport]
    public interface IClassificationService : ICompilerService
    {
        /// <summary>
        /// Returns a set of <see cref="TextSpanClassification"/> saying how a <see cref="SyntaxTree"/> must be colored
        /// in the editor.
        /// </summary>
        /// <param name="model">The <see cref="SemanticModel"/> of the <see cref="SyntaxTree"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IClassifiedTextSpans GetClassifiedTextSpans( SemanticModel model, CancellationToken cancellationToken );
    }
}