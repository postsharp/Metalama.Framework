﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.CompileTimeContracts
{
    /// <summary>
    /// Value of a template type argument. 
    /// </summary>
    [PublicAPI]
    public sealed class TemplateTypeArgument
    {
        public string Name { get; }

        public TypeSyntax Syntax { get; }

        public TypeSyntax SyntaxWithoutNullabilityAnnotations { get; }

        public IType Type { get; }

        internal TemplateTypeArgument( string name, IType type, TypeSyntax syntax, TypeSyntax syntaxWithoutNullabilityAnnotations )
        {
            this.Name = name;
            this.Syntax = syntax;
            this.Type = type;
            this.SyntaxWithoutNullabilityAnnotations = syntaxWithoutNullabilityAnnotations;
        }
    }
}