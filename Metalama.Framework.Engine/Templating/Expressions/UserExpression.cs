// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.MetaModel;
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

        protected abstract ExpressionSyntax ToSyntax( SyntaxSerializationContext syntaxSerializationContext );

        /// <summary>
        /// Creates a <see cref="TypedExpressionSyntaxImpl"/> for the given <see cref="SyntaxGenerationContext"/>.
        /// </summary>
        internal TypedExpressionSyntaxImpl ToTypedExpressionSyntax( SyntaxSerializationContext syntaxSerializationContext )
            => new(
                this.ToSyntax( syntaxSerializationContext ),
                this.Type,
                syntaxSerializationContext.CompilationModel,
                this.IsReferenceable,
                this.CanBeNull );

        public abstract IType Type { get; }

        public virtual RefKind RefKind => RefKind.None;

        public virtual bool IsAssignable => false;

        private protected virtual bool IsReferenceable => false;

        public ref object? Value => ref RefHelper.Wrap( this );

        TypedExpressionSyntax IUserExpression.ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => this.ToTypedExpressionSyntax( (SyntaxSerializationContext) syntaxGenerationContext );

        public sealed override string ToString() => this._toString ??= this.ToStringCore();

        protected virtual bool CanBeNull => true;

        protected virtual string ToStringCore()
        {
            var compilation = this.Type.GetCompilationModel();

            return
                this.ToSyntax(
                        new SyntaxSerializationContext(
                            compilation,
                            compilation.CompilationContext.GetSyntaxGenerationContext( SyntaxGenerationOptions.Formatted, isNullOblivious: false ),
                            null ) )
                    .NormalizeWhitespace()
                    .ToString();
        }
    }
}