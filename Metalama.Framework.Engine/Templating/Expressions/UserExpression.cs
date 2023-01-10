// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// Adds implementation methods to the public <see cref="IExpression"/> interface. 
    /// </summary>
    internal abstract class UserExpression : IUserExpression
    {
        private string? _toString;

        protected abstract ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext );

        /// <summary>
        /// Creates a <see cref="TypedExpressionSyntaxImpl"/> for the given <see cref="SyntaxGenerationContext"/>.
        /// </summary>
        private TypedExpressionSyntax ToTypedExpressionSyntax( SyntaxGenerationContext syntaxGenerationContext )
            => new TypedExpressionSyntaxImpl( this.ToSyntax( syntaxGenerationContext ), this.Type, syntaxGenerationContext );

        public abstract IType Type { get; }

        public RefKind RefKind => RefKind.None;

        public virtual bool IsAssignable => false;

        public ref object? Value => ref RefHelper.Wrap( this );

        TypedExpressionSyntax IUserExpression.ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => this.ToTypedExpressionSyntax( (SyntaxGenerationContext) syntaxGenerationContext );

        public sealed override string ToString() => this._toString ??= this.ToStringCore();

        protected virtual string ToStringCore()
            => this.ToSyntax( SyntaxGenerationContext.Create( this.Type.GetCompilationModel().CompilationContext, false, false ) )
                .ToString();
    }
}