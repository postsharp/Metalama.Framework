// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// Adds implementation methods to the public <see cref="IExpression"/> interface. 
    /// </summary>
    internal abstract class UserExpression : IUserExpression
    {
        protected abstract ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext );

        /// <summary>
        /// Creates a <see cref="TypedExpressionSyntax"/> for the given <see cref="SyntaxGenerationContext"/>.
        /// </summary>
        public TypedExpressionSyntax ToTypedExpressionSyntax( SyntaxGenerationContext syntaxGenerationContext )
            => new( this.ToSyntax( syntaxGenerationContext ), this.Type, syntaxGenerationContext );

        public abstract IType Type { get; }

        public virtual bool IsAssignable => false;

        public object? Value
        {
            get => this;
            set => throw new NotSupportedException();
        }
    }
}