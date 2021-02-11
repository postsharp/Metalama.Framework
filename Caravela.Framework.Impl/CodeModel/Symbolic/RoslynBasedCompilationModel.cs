using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Collections;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class RoslynBasedCompilationModel : CompilationModel
    {
        private ImmutableMultiValueDictionary<ICodeElement, IIntroducedElement> _introducedElements;

        public RoslynBasedCompilationModel( CSharpCompilation roslynCompilation ) : base( roslynCompilation )
        {
            this._introducedElements = ImmutableMultiValueDictionary<ICodeElement, IIntroducedElement>.Empty;
        }

        /// <summary>
        /// Incremental constructor. Uses the same Roslyn compilation as the prototype and append transformations.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="introducedElements"></param>
        public RoslynBasedCompilationModel( RoslynBasedCompilationModel prototype, IEnumerable<IIntroducedElement> introducedElements )
        : base( prototype.RoslynCompilation )
        {
            this._introducedElements = prototype._introducedElements.AddRange( introducedElements, t => t.ContainingElement, t => t );
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

        public override IReadOnlyMultiValueDictionary<ICodeElement, IIntroducedElement> IntroducedElements => this._introducedElements;
        protected override NamedType CreateNamedType( INamedTypeSymbol symbol ) => new NamedType( symbol, this );

        
        public IEnumerable<AspectInstance> GetAspectsFromAttributes(CompileTimeAssemblyLoader loader)
        {
            var iAspect = this.GetTypeByReflectionType( typeof( IAspect ) )!;

            var codeElements = new ICodeElement[] { this }
                .SelectDescendants( codeElement => codeElement switch
                {
                    ICompilation compilation => compilation.DeclaredTypes,
                    INamedType namedType => namedType.NestedTypes.Union<ICodeElement>( namedType.Methods ).Union( namedType.Properties ).Union( namedType.Events ),
                    IMethod method => method.LocalFunctions,
                    _ => null
                } );

            return from codeElement in codeElements
                from attribute in codeElement.Attributes
                where attribute.Type.Is( iAspect )
                let aspect = (IAspect) loader.CreateAttributeInstance( attribute )
                select new AspectInstance( aspect, codeElement, attribute.Type );
        }
    }
}
