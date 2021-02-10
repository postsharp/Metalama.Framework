using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Collections;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class CompilationModel : ICompilation
    {
        public abstract IReadOnlyList<NamedType> DeclaredTypes { get; }
        IReadOnlyList<INamedType> ICompilation.DeclaredAndReferencedTypes => this.DeclaredAndReferencedTypes;

        IReadOnlyMultiValueDictionary<string?, INamedType> ICompilation.DeclaredTypesByNamespace => this.DeclaredTypesByNamespace

        IReadOnlyList<INamedType> ICompilation.DeclaredTypes => this.DeclaredTypes;

        public abstract IReadOnlyList<NamedType> DeclaredAndReferencedTypes { get; }

        [Memo]
        public MultiValueDictionary<string?, NamedType> DeclaredTypesByNamespace
            => this.DeclaredTypes.ToMultiValueDictionary( t => t.Namespace, t => t );
            

        ICodeElement? ICodeElement.ContainingElement => null;

        IReadOnlyList<IAttribute> ICodeElement.Attributes => this.Attributes;

        public abstract IReadOnlyList<Attribute> Attributes { get; }

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public abstract INamedType? GetTypeByReflectionName( string reflectionName );
        
        /// <summary>
        /// Gets the list of transformations added by any previous layer of the compilation model.
        /// </summary>
        public abstract IReadOnlyList<Transformation> Transformations { get; }

        [Memo]
        public IReadOnlyDictionary<CodeElement, IReadOnlyList<IntroducedElement>> IntroductionsByContainingElement =>
            this.Transformations
                .OfType<IntroducedElement>()
                .GroupBy( i => i.ContainingElement )
                .ToDictionary( g => g.Key, g => (IReadOnlyList<IntroducedElement>) g.ToImmutableList() );



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

            if ( type.IsPointer )
            {
                var pointedToType = this.GetTypeByReflectionType( type.GetElementType() );

                return pointedToType?.MakePointerType();
            }

            if ( type.IsConstructedGenericType )
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


        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
        bool IEquatable<ICodeElement>.Equals( ICodeElement other ) => throw new NotImplementedException();
    }
}
