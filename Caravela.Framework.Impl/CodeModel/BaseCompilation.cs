using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    abstract class BaseCompilation : ICompilation
    {
        public abstract IReactiveCollection<INamedType> DeclaredTypes { get; }
        public abstract IReactiveCollection<INamedType> DeclaredAndReferencedTypes { get; }

        [Memo]
        public IReactiveGroupBy<string?, INamedType> DeclaredTypesByNamespace => this.DeclaredTypes.GroupBy( t => t.Namespace );

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        ICodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public abstract INamedType? GetTypeByReflectionName( string reflectionName );

        public IType? GetTypeByReflectionType( Type type )
        {
            if ( type.IsByRef )
            {
                throw new ArgumentException( "Ref types can't be represented as Caravela types." );
            }

            if ( type.IsArray )
            {
                var elementType = this.GetTypeByReflectionType( type.GetElementType() );

                return elementType?.MakeArrayType( type.GetArrayRank() );
            }

            if (type.IsPointer)
            {
                var pointedToType = this.GetTypeByReflectionType( type.GetElementType() );

                return pointedToType?.MakePointerType();
            }

            if (type.IsConstructedGenericType)
            {
                var genericDefinition = this.GetTypeByReflectionName( type.GetGenericTypeDefinition().FullName );
                var genericArguments = type.GenericTypeArguments.Select( this.GetTypeByReflectionType ).ToArray();

                if ( genericArguments.Any( a => a == null ) )
                {
                    return null;
                }

                return genericDefinition?.MakeGenericType( genericArguments! );
            }

            return this.GetTypeByReflectionName( type.FullName );
        }

        internal abstract CSharpCompilation GetPrimeCompilation();
        internal abstract IReactiveCollection<AdviceInstance> CollectAdvices();

        internal abstract CSharpCompilation GetRoslynCompilation();
        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
    }
}
