// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltParameter : BuiltCodeElement, IParameter
    {
        public BuiltParameter( IParameterBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.ParameterBuilder = builder;
        }

        public IParameterBuilder ParameterBuilder { get; }

        public override CodeElementBuilder Builder => (CodeElementBuilder) this.ParameterBuilder;

        public RefKind RefKind => this.ParameterBuilder.RefKind;

        [Memo]
        public IType ParameterType => this.Compilation.Factory.GetIType( this.ParameterBuilder.ParameterType );

        public string Name => this.ParameterBuilder.Name;

        public int Index => this.ParameterBuilder.Index;

        public TypedConstant DefaultValue => this.ParameterBuilder.DefaultValue;

        public bool IsParams => this.ParameterBuilder.IsParams;

        [Memo]
        public IMember DeclaringMember => this.Compilation.Factory.GetCodeElement( this.ParameterBuilder.DeclaringMember );
    }
}