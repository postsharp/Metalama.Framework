// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Utilities;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltParameter : BuiltDeclaration, IParameterImpl
    {
        public BuiltParameter( IParameterBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.ParameterBuilder = builder;
        }

        public IParameterBuilder ParameterBuilder { get; }

        public override DeclarationBuilder Builder => (DeclarationBuilder) this.ParameterBuilder;

        public RefKind RefKind => this.ParameterBuilder.RefKind;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this.ParameterBuilder.Type );

        public string Name => this.ParameterBuilder.Name;

        public int Index => this.ParameterBuilder.Index;

        public TypedConstant? DefaultValue => this.ParameterBuilder.DefaultValue;

        public bool IsParams => this.ParameterBuilder.IsParams;

        [Memo]
        public IHasParameters DeclaringMember => this.Compilation.Factory.GetDeclaration( this.ParameterBuilder.DeclaringMember );

        public ParameterInfo ToParameterInfo() => this.ParameterBuilder.ToParameterInfo();

        public bool IsReturnParameter => this.ParameterBuilder.IsReturnParameter;
    }
}