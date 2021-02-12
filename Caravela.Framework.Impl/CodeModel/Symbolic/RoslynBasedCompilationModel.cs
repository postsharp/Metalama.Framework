using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Collections;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class RoslynBasedCompilationModel : CompilationModel
    {
        private readonly ImmutableMultiValueDictionary<ICodeElement, IObservableTransformation> _transformations;
        private readonly ImmutableMultiValueDictionary<INamedType, IAttribute> _allAttributesByType;

        public RoslynBasedCompilationModel( CSharpCompilation roslynCompilation ) : base( roslynCompilation )
        {
            this._transformations = ImmutableMultiValueDictionary<ICodeElement, IObservableTransformation>.Empty;

            var allCodeElements = new ICodeElement[] { this }
                .SelectDescendants( codeElement => codeElement.SelectContainedElements() );

            var allAttributes = allCodeElements.SelectMany( c => c.Attributes );
            this._allAttributesByType = ImmutableMultiValueDictionary<INamedType, IAttribute>.Create( allAttributes, a => a.Type );
        }

        /// <summary>
        /// Incremental constructor. Uses the same Roslyn compilation as the prototype and append transformations.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="introducedElements"></param>
        public RoslynBasedCompilationModel( RoslynBasedCompilationModel prototype, IEnumerable<IObservableTransformation> introducedElements )
        : base( prototype.RoslynCompilation )
        {
            this._transformations = prototype._transformations.AddRange( introducedElements, t => t.ContainingElement, t => t );

            var allNewCodeElements =
                introducedElements
                    .OfType<ICodeElement>()
                    .SelectDescendants( codeElement => codeElement.SelectContainedElements() );

            var allAttributes =
                allNewCodeElements.SelectMany( c => c.Attributes )
                    .Concat( introducedElements.OfType<IAttribute>() );

            this._allAttributesByType = prototype._allAttributesByType.AddRange( allAttributes, a => a.Type );
        }

        [Memo]
        public override IReadOnlyList<INamedType> DeclaredTypes =>
            this.RoslynCompilation.Assembly.GetTypes().Select( this.GetNamedType ).ToImmutableArray();

        [Memo]
        public override IReadOnlyList<INamedType> DeclaredAndReferencedTypes =>
            this.RoslynCompilation.GetTypes().Select( this.GetNamedType ).ToImmutableArray();

        [Memo]
        public override IReadOnlyList<IAttribute> Attributes =>
            this.RoslynCompilation.Assembly.GetAttributes().Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                .Select( a => new Attribute( a, this, this ) )
                .ToImmutableArray();

        public override INamedType? GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this.RoslynCompilation.GetTypeByMetadataName( reflectionName );

            return symbol == null ? null : this.GetNamedType( symbol );
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public override IReadOnlyMultiValueDictionary<ICodeElement, IObservableTransformation> ObservableTransformations => this._transformations;

        protected override NamedType CreateNamedType( INamedTypeSymbol symbol ) => new NamedType( symbol, this );

        public override IReadOnlyMultiValueDictionary<INamedType, IAttribute> AllAttributesByType => this._allAttributesByType;
    }
}
