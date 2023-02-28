// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefKind = Metalama.Framework.Code.RefKind;

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
        internal TypedExpressionSyntaxImpl ToTypedExpressionSyntax( SyntaxGenerationContext syntaxGenerationContext )
            => new( this.ToSyntax( syntaxGenerationContext ), this.Type, syntaxGenerationContext, false, this.CanBeNull );

        public abstract IType Type { get; }

        public RefKind RefKind => RefKind.None;

        public virtual bool IsAssignable => false;

        public ref object? Value => ref RefHelper.Wrap( this );

        TypedExpressionSyntax IUserExpression.ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => this.ToTypedExpressionSyntax( (SyntaxGenerationContext) syntaxGenerationContext );

        public sealed override string ToString() => this._toString ??= this.ToStringCore();

        protected virtual bool CanBeNull => true;

        protected virtual string ToStringCore()
            => this.ToSyntax( SyntaxGenerationContext.Create( this.Type.GetCompilationModel().CompilationContext, false, false ) )
                .NormalizeWhitespace()
                .ToString();
    }
}