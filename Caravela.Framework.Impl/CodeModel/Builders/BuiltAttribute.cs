using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltAttribute : BuiltCodeElement, IAttribute, ICodeElementLink<IAttribute>
    {
        public BuiltAttribute(AttributeBuilder builder, CompilationModel compilation) : base(compilation)
        {
            this.AttributeBuilder = builder;
        }
        
        public AttributeBuilder AttributeBuilder { get; }

        public override CodeElementBuilder Builder => this.AttributeBuilder;

        [Memo]
        public INamedType Type => this.Compilation.Factory.GetCodeElement( this.AttributeBuilder.Constructor.DeclaringType );

        [Memo]
        public IMethod Constructor => this.Compilation.Factory.GetMethod( this.AttributeBuilder.Constructor );

        public IReadOnlyList<object?> ConstructorArguments => this.AttributeBuilder.ConstructorArguments;

        public INamedArgumentList NamedArguments => this.AttributeBuilder.NamedArguments;

        public IAttribute GetForCompilation( CompilationModel compilation ) =>
            compilation == this.Compilation ? this : throw new AssertionFailedException();
    }
}