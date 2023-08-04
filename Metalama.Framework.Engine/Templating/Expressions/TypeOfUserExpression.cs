// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal sealed class TypeOfUserExpression : UserExpression
    {
        private readonly IType _type;

        public TypeOfUserExpression( IType type )
        {
            this._type = type;
        }

        protected override ExpressionSyntax ToSyntax( SyntaxSerializationContext syntaxSerializationContext )
            => syntaxSerializationContext.SyntaxGenerator.TypeOfExpression( this._type.GetSymbol() );

        protected override bool CanBeNull => false;

        public override IType Type => ((ICompilationInternal) this._type.Compilation).Factory.GetTypeByReflectionType( typeof(System.Type) );
    }
}