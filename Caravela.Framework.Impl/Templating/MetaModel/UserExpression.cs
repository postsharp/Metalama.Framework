using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class UserExpression : IExpression
    {
        public IDynamicExpression Underlying { get; }

        public UserExpression( RuntimeExpression? underlying, ICompilation compilation )
        {
            if ( underlying == null )
            {
                this.Underlying = new DefaultDynamicExpression( compilation.TypeFactory.GetSpecialType( SpecialType.Object ) );
            }
            else
            {
                var type = underlying.ExpressionType != null
                    ? compilation.GetCompilationModel().Factory.GetIType( underlying.ExpressionType )
                    : compilation.TypeFactory.GetSpecialType( SpecialType.Object ).MakeNullable();

                this.Underlying = new DynamicExpression( underlying.Syntax, type, false );
            }
        }

        public IType Type => this.Underlying.ExpressionType;

        public bool IsAssignable => this.Underlying.IsAssignable;

        public object? Value
        {
            get => this.ToSyntax();
            set => throw new NotSupportedException();
        }

        public ISyntax ToSyntax() => this.Underlying;
    }
}