using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal partial class CompilationModel : ICompilation
    {
        private readonly ImmutableMultiValueDictionary<ICodeElement, IObservableTransformation> _transformations;
        private readonly ImmutableMultiValueDictionary<INamedType, IAttribute> _allAttributesByType;

        public CodeElementFactory Factory { get; }

        public CompilationModel( CSharpCompilation roslynCompilation )
        {
            this.RoslynCompilation = roslynCompilation;
            this._transformations = ImmutableMultiValueDictionary<ICodeElement, IObservableTransformation>.Empty.WithKeyComparer( CodeElementEqualityComparer.Instance );
            this.Factory = new CodeElementFactory( this );

            var allCodeElements = this.SelectContainedElements();

            var allAttributes = allCodeElements.SelectMany( c => c.Attributes );
            this._allAttributesByType = ImmutableMultiValueDictionary<INamedType, IAttribute>.Create( allAttributes, a => a.Type, CodeElementEqualityComparer.Instance );
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
            this.Factory = new CodeElementFactory( this );

            var allNewCodeElements =
                introducedElements
                    .OfType<ICodeElement>()
                    .SelectDescendants( codeElement => codeElement.SelectContainedElements() );

            var allAttributes =
                allNewCodeElements.SelectMany( c => c.Attributes )
                    .Concat( introducedElements.OfType<IAttribute>() );

            this._allAttributesByType = prototype._allAttributesByType.AddRange( allAttributes, a => a.Type );
        }

        public CSharpSyntaxGenerator SyntaxGenerator { get; } = new CSharpSyntaxGenerator();

        [Memo]
        public IReadOnlyList<INamedType> DeclaredTypes =>
            this.RoslynCompilation.Assembly.GetTypes().Select( this.Factory.GetNamedType ).ToImmutableArray();

        [Memo]
        public IReadOnlyList<INamedType> DeclaredAndReferencedTypes =>
            this.RoslynCompilation.GetTypes().Select( this.Factory.GetNamedType ).ToImmutableArray();

        [Memo]
        public IReadOnlyList<IAttribute> Attributes =>
            this.RoslynCompilation.Assembly.GetAttributes().Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                .Select( a => new Attribute( a, this, this ) )
                .ToImmutableArray();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public IReadOnlyMultiValueDictionary<ICodeElement, IObservableTransformation> ObservableTransformations => this._transformations;

        public IReadOnlyMultiValueDictionary<INamedType, IAttribute> AllAttributesByType => this._allAttributesByType;

        internal CSharpCompilation RoslynCompilation { get; }

        [Memo]
        public IReadOnlyMultiValueDictionary<string, INamedType> DeclaredTypesByNamespace
            => this.DeclaredTypes.ToMultiValueDictionary( t => t.Namespace ?? string.Empty, t => t );

        ITypeFactory ICompilation.TypeFactory => this.Factory;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        CodeOrigin ICodeElement.Origin => CodeOrigin.Source;

        ICodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();

        ICompilation ICodeElement.Compilation => this;

        public IDiagnosticLocation? DiagnosticLocation => null;
    }
}
