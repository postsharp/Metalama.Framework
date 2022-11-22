// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
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
        /// Creates a <see cref="TypedExpressionSyntaxImpl"/> for the given <see cref="SyntaxGenerationContext"/>.
        /// </summary>
        public TypedExpressionSyntax ToTypedExpressionSyntax( SyntaxGenerationContext syntaxGenerationContext )
            => new TypedExpressionSyntaxImpl( this.ToSyntax( syntaxGenerationContext ), this.Type, syntaxGenerationContext );

        public abstract IType Type { get; }

        public virtual bool IsAssignable => false;

        public object? Value
        {
            get => this;
            set => throw new NotSupportedException();
        }

        TypedExpressionSyntax IUserExpression.ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => this.ToTypedExpressionSyntax( (SyntaxGenerationContext) syntaxGenerationContext );
    }
}