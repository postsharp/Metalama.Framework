using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkedCompilationModel : CompilationModel
    {
        private CompilationModel _originalCompilation;

        public LinkedCompilationModel(CompilationModel originalCompilation)
        {
            this._originalCompilation = originalCompilation;
        }

        public override IReactiveCollection<INamedType> DeclaredTypes => this._originalCompilation.DeclaredTypes;

        public override IReactiveCollection<INamedType> DeclaredAndReferencedTypes => this._originalCompilation.DeclaredAndReferencedTypes;

        public override IReactiveCollection<IAttribute> Attributes => this._originalCompilation.Attributes;

        public override INamedType? GetTypeByReflectionName( string reflectionName ) => this._originalCompilation.GetTypeByReflectionName( reflectionName );

        internal override CSharpCompilation GetPrimeCompilation() => this._originalCompilation.GetPrimeCompilation();

        internal override IReactiveCollection<Transformation> CollectTransformations() => this._originalCompilation.CollectTransformations();

        internal override CSharpCompilation GetRoslynCompilation()
        {
            return this._originalCompilation.GetRoslynCompilation();
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._originalCompilation.ToDisplayString( format, context );
    }
}
