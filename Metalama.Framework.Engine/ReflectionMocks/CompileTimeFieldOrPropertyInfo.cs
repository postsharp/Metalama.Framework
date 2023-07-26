// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.RunTime;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal sealed class CompileTimeFieldOrPropertyInfo : FieldOrPropertyInfo, IUserExpression
    {
        public IFieldOrPropertyOrIndexer FieldOrPropertyOrIndexer { get; }

        private CompileTimeFieldOrPropertyInfo( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer )
        {
            this.FieldOrPropertyOrIndexer = fieldOrPropertyOrIndexer;
        }

        public static FieldOrPropertyInfo Create( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer )
            => new CompileTimeFieldOrPropertyInfo( fieldOrPropertyOrIndexer );

        public bool IsAssignable => false;

        public IType Type => TypeFactory.GetType( typeof(FieldOrPropertyInfo) );

        public RefKind RefKind => RefKind.None;

        public ref object? Value => ref RefHelper.Wrap( this );

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        {
            var generationContext = (SyntaxGenerationContext) syntaxGenerationContext;

            var compilation = UserCodeExecutionContext.Current.Compilation.AssertNotNull();

            var expression = CompileTimeFieldOrPropertyInfoSerializer.SerializeFieldOrProperty(
                this.FieldOrPropertyOrIndexer,
                new( compilation, generationContext ) );

            return new(
                new TypedExpressionSyntaxImpl(
                    expression,
                    this.Type,
                    generationContext,
                    true ) );
        }
    }
}