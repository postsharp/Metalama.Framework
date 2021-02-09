using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class SourceCompilationModel : CompilationModel
    {
        internal CSharpCompilation RoslynCompilation { get; }

        internal SymbolMap SymbolMap { get; }

        public SourceCompilationModel( CSharpCompilation roslynCompilation )
        {
            this.RoslynCompilation = roslynCompilation;

            this.SymbolMap = new ( this );
        }

        public override IImmutableList<Transformation> Transformations => ImmutableArray<Transformation>.Empty;

        [Memo]
        public override IReactiveCollection<INamedType> DeclaredTypes =>
            this.RoslynCompilation.Assembly.GetTypes().Select( this.SymbolMap.GetNamedType ).ToImmutableReactive();

        [Memo]
        public override IReactiveCollection<INamedType> DeclaredAndReferencedTypes =>
            this.RoslynCompilation.GetTypes().Select( this.SymbolMap.GetNamedType ).ToImmutableReactive();

        [Memo]
        public override IImmutableList<IAttribute> Attributes =>
            this.RoslynCompilation.Assembly.GetAttributes().Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                .Select( a => new Attribute( a, this.SymbolMap ) )
                .ToImmutableReactive();

        public override INamedType? GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this.RoslynCompilation.GetTypeByMetadataName( reflectionName );

            return symbol == null ? null : this.SymbolMap.GetNamedType( symbol );
        }
        
        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";
    }
}
