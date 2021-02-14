using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Collections;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal partial class CompilationModel : ICompilation, ITypeFactory
    {
        private readonly ImmutableMultiValueDictionary<ICodeElement, IObservableTransformation> _transformations;
        private readonly ImmutableMultiValueDictionary<INamedType, IAttribute> _allAttributesByType;

        public CompilationModel( CSharpCompilation roslynCompilation )
        {
            this.RoslynCompilation = roslynCompilation;
            this._transformations = ImmutableMultiValueDictionary<ICodeElement, IObservableTransformation>.Empty;

            var allCodeElements = this.SelectContainedElements();

            var allAttributes = allCodeElements.SelectMany( c => c.Attributes );
            this._allAttributesByType = ImmutableMultiValueDictionary<INamedType, IAttribute>.Create( allAttributes, a => a.Type );
        }

        /// <summary>
        /// Incremental constructor. Uses the same Roslyn compilation as the prototype and append transformations.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="introducedElements"></param>
        public CompilationModel( CompilationModel prototype, IEnumerable<IObservableTransformation> introducedElements )
        {
            this.RoslynCompilation = prototype.RoslynCompilation;
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
        public IReadOnlyList<INamedType> DeclaredTypes =>
            this.RoslynCompilation.Assembly.GetTypes().Select( this.GetNamedType ).ToImmutableArray();

        [Memo]
        public IReadOnlyList<INamedType> DeclaredAndReferencedTypes =>
            this.RoslynCompilation.GetTypes().Select( this.GetNamedType ).ToImmutableArray();

        [Memo]
        public IReadOnlyList<IAttribute> Attributes =>
            this.RoslynCompilation.Assembly.GetAttributes().Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                .Select( a => new Attribute( a, this, this ) )
                .ToImmutableArray();

        public INamedType? GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this.RoslynCompilation.GetTypeByMetadataName( reflectionName );

            return symbol == null ? null : this.GetNamedType( symbol );
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public IReadOnlyMultiValueDictionary<ICodeElement, IObservableTransformation> ObservableTransformations => this._transformations;

        public IReadOnlyMultiValueDictionary<INamedType, IAttribute> AllAttributesByType => this._allAttributesByType;

        internal CSharpCompilation RoslynCompilation { get; }

        [Memo]
        public IReadOnlyMultiValueDictionary<string, INamedType> DeclaredTypesByNamespace
            => this.DeclaredTypes.ToMultiValueDictionary( t => t.Namespace ?? string.Empty, t => t );

        ITypeFactory ICompilation.TypeFactory => this;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        ICodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();
    }
}
