// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.CompileTimeContracts
{
    /// <summary>
    /// Value of a template type argument. 
    /// </summary>
    public sealed class TemplateTypeArgument
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