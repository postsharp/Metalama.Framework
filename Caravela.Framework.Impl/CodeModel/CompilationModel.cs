using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationModel : ICompilation, ICodeElementLink<ICompilation>
    {
        public static CompilationModel CreateInitialInstance( CSharpCompilation roslynCompilation )
        {
            return new CompilationModel(roslynCompilation);
        }

        public static CompilationModel CreateRevisedInstance( CompilationModel prototype, IEnumerable<IObservableTransformation> introducedElements )
        {
            if ( !introducedElements.Any() )
            {
                return prototype;
            }
            
            return new CompilationModel(prototype, introducedElements);
        }

        private readonly ImmutableMultiValueDictionary<CodeElementLink<ICodeElement>, IObservableTransformation> _transformations;
        private readonly ImmutableMultiValueDictionary<CodeElementLink<INamedType>, AttributeLink> _allAttributesByType;
        private readonly ImmutableMultiValueDictionary<CodeElementLink<INamedType>, CodeElementLink<ICodeElement>> _allAspectExclusions;
        private ImmutableDictionary<CodeElementLink<ICodeElement>, int> _depthsCache = ImmutableDictionary.Create<CodeElementLink<ICodeElement>, int>();

        public CodeElementFactory Factory { get; }

        private CompilationModel( CSharpCompilation roslynCompilation )
        {
            this.RoslynCompilation = roslynCompilation;
            
            this._transformations = ImmutableMultiValueDictionary<CodeElementLink<ICodeElement>, IObservableTransformation>
                .Empty
                .WithKeyComparer( CodeElementLinkEqualityComparer<CodeElementLink<ICodeElement>>.Instance );
            
            this.Factory = new CodeElementFactory( this );

            var assembly = new [] { roslynCompilation.Assembly};
            var allCodeElements = assembly.Concat( assembly.SelectDescendants<ISymbol>( s => s.GetContainedSymbols() ) );
            
            var allAttributes = allCodeElements.SelectMany( c => c.GetAllAttributes() );
            this._allAttributesByType = ImmutableMultiValueDictionary<CodeElementLink<INamedType>, AttributeLink>
                .Create( allAttributes, a => a.AttributeType, CodeElementLinkEqualityComparer<CodeElementLink<INamedType>>.Instance );
            
        }

        /// <summary>
        /// Incremental constructor. Uses the same Roslyn compilation as the prototype and append transformations.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="introducedElements"></param>
        private CompilationModel( CompilationModel prototype, IEnumerable<IObservableTransformation> introducedElements )
        {
            this.Revision = prototype.Revision + 1;
            this.RoslynCompilation = prototype.RoslynCompilation;
            
            this._transformations = prototype._transformations.AddRange(
                introducedElements, 
                t => new CodeElementLink<ICodeElement>( t.ContainingElement ), 
                t => t );
            
            this.Factory = new CodeElementFactory( this );

            var allNewCodeElements =
                introducedElements
                    .OfType<ICodeElement>()
                    .SelectDescendants( codeElement => codeElement.GetContainedElements() );

            var allAttributes =
                allNewCodeElements.SelectMany( c => c.Attributes )
                    .Cast<IAttributeLink>() // We actually have AttributeBuilders here.
                    .Concat( introducedElements.OfType<IAttributeLink>() )
                    .Select( a => new AttributeLink( a ) );
            
            // TODO: this cache may need to be smartly invalidated when we have interface introductions.
            this._depthsCache = prototype._depthsCache;

            this._allAttributesByType = prototype._allAttributesByType.AddRange( allAttributes, a => a.AttributeType );
        }

        public CSharpSyntaxGenerator SyntaxGenerator { get; } = new CSharpSyntaxGenerator();

        public int Revision { get; }

        [Memo]
        public INamedTypeList DeclaredTypes =>
            new NamedTypeList(
                this.RoslynCompilation.Assembly
                    .GetTypes()
                    .Select( t => new MemberLink<INamedType>( t ) ),
                this );

        [Memo]
        public IReadOnlyList<INamedType> DeclaredAndReferencedTypes =>
            this.RoslynCompilation.GetTypes().Select( this.Factory.GetNamedType ).ToImmutableArray();

        [Memo]
        public IAttributeList Attributes =>
            new AttributeList(
                this.RoslynCompilation.Assembly
                    .GetAttributes()
                    .Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                    .Select( 
                        a => new AttributeLink( a, this.RoslynCompilation.Assembly.ToLink() ) ),
                        this );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        internal CSharpCompilation RoslynCompilation { get; }
        

        ITypeFactory ICompilation.TypeFactory => this.Factory;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        ICodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();

        ICompilation ICodeElement.Compilation => this;

        public IDiagnosticLocation? DiagnosticLocation => null;
        
        
        public IEnumerable<INamedType> GetAllAttributeTypes() 
            => this._allAttributesByType.Keys.Select( t => t.GetForCompilation( this ) );
        public IEnumerable<IAttribute> GetAllAttributesOfType( INamedType type )
            => this._allAttributesByType[new CodeElementLink<INamedType>( type )].Select( a => a.GetForCompilation( this ) );

        public ImmutableArray<IObservableTransformation> GetObservableTransformationsOnElement( ICodeElement codeElement )
            => this._transformations[new CodeElementLink<ICodeElement>( codeElement )];

        public IEnumerable<(ICodeElement DeclaringElement, IEnumerable<IObservableTransformation> Transformations)> GetAllObservableTransformations()
        {
            foreach ( var group in this._transformations )
            {
                yield return (group.Key.GetForCompilation( this ), group);
            }
        }

        public int GetDepth( ICodeElement codeElement )
        {
            var link = CodeElementLink.FromLink( (ICodeElementLink<ICodeElement>) codeElement );
            
            if ( this._depthsCache.TryGetValue( link, out var value ) )
            {
                return value;
            }
            else
            {
                switch (codeElement)
                {
                    case INamedType namedType:
                        return this.GetDepth( namedType );
                    
                    case ICompilation:
                        return 0;
                    
                    default:
                    {
                        var depth = this.GetDepth( codeElement.ContainingElement! ) + 1;
                        this._depthsCache = this._depthsCache.SetItem( link, depth );
                        return depth;
                    }
                }
            }
        }
        
        public int GetDepth( INamedType namedType )
        {
            var link = CodeElementLink.FromLink( (ICodeElementLink<ICodeElement>) namedType );
            
            if ( this._depthsCache.TryGetValue( link, out var depth ) )
            {
                return depth;
            }
            else
            {
                depth = this.GetDepth( namedType.ContainingElement! );
                
                if ( namedType.BaseType != null )
                {
                    depth = Math.Max( depth, this.GetDepth( namedType.BaseType ) );
                }

                foreach ( var interfaceImplementation in namedType.ImplementedInterfaces )
                {
                    depth = Math.Max( depth, this.GetDepth( interfaceImplementation ) );
                }

                depth++;
              
                this._depthsCache = this._depthsCache.SetItem( link, depth );

                return depth;
            }
        }

        public ICompilation GetForCompilation( CompilationModel compilation ) => compilation;

        public object? Target => this.RoslynCompilation;
    }
}
