// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Advices
{
    /// <summary>
    /// Value of a template type argument. 
    /// </summary>
    internal class TemplateTypeArgument
    {
        public ExpressionSyntax Syntax { get; }

        public IType Type { get; }

        public TemplateTypeArgument( ExpressionSyntax syntax, IType type )
        {
            this.Syntax = syntax;
            this.Type = type;
        }
    }
}