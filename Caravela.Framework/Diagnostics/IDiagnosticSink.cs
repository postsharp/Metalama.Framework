// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.CodeFixes;
using Caravela.Framework.Validation;
using System.Collections.Generic;

namespace Caravela.Framework.Diagnostics
{
    public readonly struct CodeFix
    {
        public string Title { get; }

        public CodeFixAsyncAction Action { get; }

        public CodeFix( string title, CodeFixAsyncAction action )
        {
            this.Title = title;
            this.Action = action;
        }
    }

    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    [CompileTimeOnly]
    [InternalImplement]
    public interface IDiagnosticSink
    {
        /// <summary>
        /// Reports a parameterless diagnostic by specifying its location.
        /// </summary>
        /// <param name="location">The code location to which the diagnostic should be written.</param>
        /// <param name="definition"></param>
        /// <param name="codeFixes"></param>
        void Report( IDiagnosticLocation? location, DiagnosticDefinition definition, IEnumerable<CodeFix>? codeFixes = null );

        /// <summary>
        /// Reports a parametric diagnostic by specifying its location.
        /// </summary>
        /// <param name="location">The code location to which the diagnostic should be written.</param>
        /// <param name="definition"></param>
        /// <param name="arguments"></param>
        /// <param name="codeFixes"></param>
        void Report<T>(
            IDiagnosticLocation? location,
            DiagnosticDefinition<T> definition,
            T arguments,
            IEnumerable<CodeFix>? codeFixes = null )
            where T : notnull;

        /// <summary>
        /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
        /// </summary>
        /// <param name="scope">The declaration in which the diagnostic must be suppressed.</param>
        /// <param name="definition"></param>
        void Suppress( IDeclaration? scope, SuppressionDefinition definition );
    }
}