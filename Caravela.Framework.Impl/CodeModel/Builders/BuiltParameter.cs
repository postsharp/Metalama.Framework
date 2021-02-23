using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltParameter : BuiltCodeElement, IParameter
    {
        public BuiltParameter( ParameterBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.ParameterBuilder = builder;
        }

        public ParameterBuilder ParameterBuilder { get; }

        public override CodeElementBuilder Builder => this.ParameterBuilder;

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