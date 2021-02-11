using System;
using Caravela.Framework.Code;
using Caravela.Framework.Collections;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract partial class CompilationModel : ICompilation, ITypeFactory
    {
        internal CSharpCompilation RoslynCompilation { get; }

        public abstract IReadOnlyList<INamedType> DeclaredTypes { get; }

        public abstract IReadOnlyList<INamedType> DeclaredAndReferencedTypes { get; }

        [Memo]
        public IReadOnlyMultiValueDictionary<string?, INamedType> DeclaredTypesByNamespace 
            => this.DeclaredTypes.ToMultiValueDictionary( t => t.Namespace, t => t );

        ITypeFactory ICompilation.TypeFactory => this;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        public abstract IReadOnlyList<IAttribute> Attributes { get; }

        ICodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public abstract INamedType? GetTypeByReflectionName( string reflectionName );



        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
        public bool Equals( ICodeElement other ) => throw new NotImplementedException();
        
        protected CompilationModel( CSharpCompilation roslynCompilation )
        {
            this.RoslynCompilation = roslynCompilation;
        }
        
        public abstract IReadOnlyMultiValueDictionary<ICodeElement?, IObservableTransformation> ObservableTransformations { get; }
    }
}
