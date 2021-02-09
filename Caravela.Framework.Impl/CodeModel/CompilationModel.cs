using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class CompilationModel : ICompilation
    {
        public abstract IReactiveCollection<INamedType> DeclaredTypes { get; }

        public abstract IReactiveCollection<INamedType> DeclaredAndReferencedTypes { get; }

        [Memo]
        public IReactiveGroupBy<string?, INamedType> DeclaredTypesByNamespace => this.DeclaredTypes.GroupBy( t => t.Namespace );

        public abstract IImmutableList<Attribute> Attributes { get; }

        CodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public abstract INamedType? GetTypeByReflectionName( string reflectionName );
        
        /// <summary>
        /// Gets the list of transformations added by any previous layer of the compilation model.
        /// </summary>
        public abstract IImmutableList<Transformation> Transformations { get; }

        [Memo]
        public IReadOnlyDictionary<CodeElement, IImmutableList<IntroducedElement>> IntroductionsByContainingElement =>
            this.Transformations.OfType<IntroducedElement>().GroupBy( i => i.ContainingElement )
                .ToDictionary<CodeElement, IImmutableList<IntroducedElement>>( g => g.Key, g => g.ToImmutableList() );

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
    }
}
