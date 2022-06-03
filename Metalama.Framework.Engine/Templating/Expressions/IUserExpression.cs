// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// Adds implementation methods to the public <see cref="IExpression"/> interface. 
    /// </summary>
    public interface IUserExpression : IExpression
    {
        ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext );

        /// <summary>
        /// Creates a <see cref="RunTimeTemplateExpression"/> for the current <see cref="TemplateExpansionContext"/>.
        /// </summary>
        /// <param name="syntaxGenerationContext"></param>
        RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext );
    }
}