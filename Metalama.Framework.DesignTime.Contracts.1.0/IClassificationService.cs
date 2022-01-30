// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Exposes a method <see cref="GetClassifiedTextSpans"/> that returns a set of <see cref="DesignTimeClassifiedTextSpan"/>
    /// saying how a <see cref="SyntaxTree"/> must be colored in the editor.
    /// </summary>
    public interface IClassificationService : ICompilerService
    {
        /// <summary>
        /// Determines whether a syntax tree possibly contains compile-time code. The result is evaluated using syntax only, without
        /// getting a semantic model, and should be called before calling <see cref="GetClassifiedTextSpans"/>.
        /// </summary>
        bool ContainsCompileTimeCode( SyntaxNode syntaxRoot );

        /// <summary>
        /// Returns a set of <see cref="DesignTimeTextSpanClassification"/> saying how a <see cref="SyntaxTree"/> must be colored
        /// in the editor.
        /// </summary>
        /// <param name="model">The <see cref="SemanticModel"/> of the <see cref="SyntaxTree"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IDesignTimeClassifiedTextCollection GetClassifiedTextSpans( SemanticModel model, CancellationToken cancellationToken );
    }
}