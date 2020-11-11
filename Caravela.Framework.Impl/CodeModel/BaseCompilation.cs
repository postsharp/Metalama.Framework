using System;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    public abstract class BaseCompilation : ICompilation
    {
        public abstract IReactiveCollection<INamedType> DeclaredTypes { get; }
        public abstract IReactiveCollection<INamedType> DeclaredAndReferencedTypes { get; }

        [Memo]
        public IReactiveGroupBy<string?, INamedType> DeclaredTypesByNamespace => this.DeclaredTypes.GroupBy( t => t.Namespace );

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        ICodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.Kind => CodeElementKind.Compilation;

        public abstract INamedType? GetTypeByReflectionName( string reflectionName );

        // TODO: add support for other kinds of types
        public IType? GetTypeByReflectionType( Type type )
        {
            if ( type.IsArray )
            {
                var elementType = this.GetTypeByReflectionType( type.GetElementType() );

                return elementType?.MakeArrayType( type.GetArrayRank() );
            }

            return this.GetTypeByReflectionName( type.FullName );
        }

        internal abstract CSharpCompilation GetPrimeCompilation();
        internal abstract IReactiveCollection<AdviceInstance> CollectAdvices();

        internal abstract CSharpCompilation GetRoslynCompilation();
    }
}
