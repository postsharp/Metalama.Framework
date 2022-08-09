// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating
{
    /// <summary>
    /// Value of a template type argument. 
    /// </summary>
    public class TemplateTypeArgument
    {
        public TypeSyntax Syntax { get; }

        public TypeSyntax SyntaxWithoutNullabilityAnnotations { get; }

        public IType Type { get; }

        internal TemplateTypeArgument( IType type, TypeSyntax syntax, TypeSyntax syntaxWithoutNullabilityAnnotations )
        {
            this.Syntax = syntax;
            this.Type = type;
            this.SyntaxWithoutNullabilityAnnotations = syntaxWithoutNullabilityAnnotations;
        }
    }
}