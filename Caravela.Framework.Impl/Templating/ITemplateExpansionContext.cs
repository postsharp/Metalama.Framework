using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Provides information about the element of code to which a template was applied
    /// and exposes methods that adapt the expanding template to the target context.
    /// </summary>
    /// <remarks>
    /// The template driver requires an instance of the expansion context before it can invoke the template method.
    /// </remarks>
    internal interface ITemplateExpansionContext 
    {
        /// <summary>
        /// Gets the element of code to which a template was applied.
        /// </summary>
        ICodeElement TargetDeclaration { get; }

        /// <summary>
        /// Gets the object on which to invoke the template method.
        /// </summary>
        object TemplateInstance { get; }

        /// <summary>
        /// Gets the implementation object which handles the proceed() calls in the template method.
        /// </summary>
        IProceedImpl ProceedImplementation { get; }

        /// <summary>
        /// Gets the whole code model which contains the target code element.
        /// </summary>
        ICompilation Compilation { get; }

        /// <summary>
        /// Gets or sets the lexical scope which allows the template method to define new unique identifiers within the target code element.
        /// </summary>
        ITemplateExpansionLexicalScope CurrentLexicalScope { get; }

        /// <summary>
        /// Creates a syntax node for a return statement within the target code element.
        /// </summary>
        /// <param name="returnExpression">The expression which represents a return value.</param>
        /// <returns>
        /// A <see cref="StatementSyntax"/> instance that will be used in place of the return statement by the template method.
        /// </returns>
        StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression );
        
        DiagnosticSink DiagnosticSink { get; }

        IDisposable OpenNestedScope();
    }
}
