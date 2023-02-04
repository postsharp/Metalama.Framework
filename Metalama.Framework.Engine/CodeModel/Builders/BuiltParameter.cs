// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class BuiltParameter : BuiltDeclaration, IParameterImpl
    {
        private readonly IParameterBuilder _parameterBuilder;

        public BuiltParameter( IParameterBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this._parameterBuilder = builder;
        }

        public override DeclarationBuilder Builder => (DeclarationBuilder) this._parameterBuilder;

        public RefKind RefKind => this._parameterBuilder.RefKind;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._parameterBuilder.Type );

        public string Name => this._parameterBuilder.Name;

        public int Index => this._parameterBuilder.Index;

        public TypedConstant? DefaultValue => this._parameterBuilder.DefaultValue;

        public bool IsParams => this._parameterBuilder.IsParams;

        [Memo]
        public IHasParameters DeclaringMember => this.Compilation.Factory.GetDeclaration( this._parameterBuilder.DeclaringMember );

        public ParameterInfo ToParameterInfo() => this._parameterBuilder.ToParameterInfo();

        public bool IsReturnParameter => this._parameterBuilder.IsReturnParameter;

        bool IExpression.IsAssignable => true;

        public ref object? Value => ref RefHelper.Wrap( new SyntaxUserExpression( SyntaxFactory.IdentifierName( this.Name ), this.Type, true ) );
    }
}