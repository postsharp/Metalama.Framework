using Caravela.Framework.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
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

        public override IReadOnlyList<Transformation> Transformations => ImmutableArray<Transformation>.Empty;

        [Memo]
        public override IReadOnlyList<NamedType> DeclaredTypes =>
            this.RoslynCompilation.Assembly.GetTypes().Select( this.SymbolMap.GetNamedType ).ToList();

        [Memo]
        public override IReadOnlyList<NamedType> DeclaredAndReferencedTypes =>
            this.RoslynCompilation.GetTypes().Select( this.SymbolMap.GetNamedType ).ToList();

        [Memo]
        public override IReadOnlyList<Attribute> Attributes =>
            this.RoslynCompilation.Assembly.GetAttributes().Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                .Select( a => new SourceAttribute( a, this.SymbolMap ) )
                .ToList();

        public override INamedType? GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this.RoslynCompilation.GetTypeByMetadataName( reflectionName );

            return symbol == null ? null : this.SymbolMap.GetNamedType( symbol );
        }
        
        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";
        
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
