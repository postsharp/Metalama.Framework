using System;
using System.Collections.Concurrent;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Serialization;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Creates instances of <see cref="ICodeElement"/> for a given <see cref="CompilationModel"/>.
    /// </summary>
    internal class CodeElementFactory : ITypeFactory
    {
        private readonly CompilationModel _compilation;
        private readonly ConcurrentDictionary<CodeElementLink<ICodeElement>, object> _cache = new( CodeElementLinkEqualityComparer<CodeElementLink<ICodeElement>>.Instance );

        public CodeElementFactory( CompilationModel compilation )
        {
            this._compilation = compilation;
        }
        
        public ObjectSerializers Serializers { get; } = new();

        private Compilation RoslynCompilation => this._compilation.RoslynCompilation;

        public INamedType GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this._compilation.ReflectionMapper.GetTypeByReflectionName( reflectionName );
            return this.GetNamedType( symbol );
        }

        public IType GetTypeByReflectionType( Type type )
            => this.GetIType( this._compilation.ReflectionMapper.GetTypeSymbol( type ) );
        
        internal IAssembly GetAssembly( IAssemblySymbol assemblySymbol )
            => (IAssembly) this._cache.GetOrAdd(
                assemblySymbol.ToLink(),
                l => !SymbolEqualityComparer.Default.Equals( l.Symbol, this._compilation.RoslynCompilation.Assembly ) ?
                    new ReferencedAssembly( (IAssemblySymbol) l.Symbol!, this._compilation )
                    : this._compilation );

        internal IType GetIType( ITypeSymbol typeSymbol )
            => (IType) this._cache.GetOrAdd( typeSymbol.ToLink(), l => CodeModelFactory.CreateIType( (ITypeSymbol) l.Symbol!, this._compilation ) );

        internal NamedType GetNamedType( INamedTypeSymbol typeSymbol )
            => (NamedType) this._cache.GetOrAdd( typeSymbol.ToLink(), s => new NamedType( (INamedTypeSymbol) s.Symbol!, this._compilation ) );

        internal GenericParameter GetGenericParameter( ITypeParameterSymbol typeParameterSymbol )
            => (GenericParameter) this._cache.GetOrAdd( typeParameterSymbol.ToLink(), tp => new GenericParameter( (ITypeParameterSymbol) tp.Symbol!, this._compilation ) );

        internal IMethod GetMethod( IMethodSymbol methodSymbol )
            => (IMethod) this._cache.GetOrAdd( methodSymbol.ToLink(), ms => new Method( (IMethodSymbol) ms.Symbol!, this._compilation ) );

        internal IProperty GetProperty( IPropertySymbol propertySymbol )
            => (IProperty) this._cache.GetOrAdd( propertySymbol.ToLink(), ms => new Property( (IPropertySymbol) ms.Symbol!, this._compilation ) );

        internal IProperty GetProperty( IFieldSymbol propertySymbol )
            => (IProperty) this._cache.GetOrAdd( propertySymbol.ToLink(), ms => new Field( (IFieldSymbol) ms.Symbol!, this._compilation ) );

        internal IParameter GetParameter( IParameterSymbol parameterSymbol, Member declaringMember )
            => (IParameter) this._cache.GetOrAdd( parameterSymbol.ToLink(), ms => new Parameter( (IParameterSymbol) ms.Symbol!, this._compilation ) );

        internal IConstructor GetConstructor( IMethodSymbol methodSymbol )
            => (IConstructor) this._cache.GetOrAdd( methodSymbol.ToLink(), ms => new Constructor( (IMethodSymbol) ms.Symbol!, this._compilation ) );

        internal IParameter GetParameter( IParameterSymbol parameterSymbol )
            => (IParameter) this._cache.GetOrAdd( parameterSymbol.ToLink(), ms => new Parameter( (IParameterSymbol) ms.Symbol!, this._compilation ) );

        internal IEvent GetEvent( IEventSymbol @event )
            => (IEvent) this._cache.GetOrAdd( @event.ToLink(), ms => new Event( (IEventSymbol) ms.Symbol!, this._compilation ) );

        internal ICodeElement GetCodeElement( ISymbol symbol, CodeElementSpecialKind kind = CodeElementSpecialKind.Default) =>
            symbol switch
            {
                INamespaceSymbol => this._compilation,
                INamedTypeSymbol namedType => this.GetNamedType( namedType ),
                IMethodSymbol method =>
                    kind == CodeElementSpecialKind.ReturnParameter 
                        ? this.GetReturnParameter( method )
                        : method.GetCodeElementKind() == CodeElementKind.Method 
                            ? this.GetMethod( method )
                            : this.GetConstructor( method ),
                IPropertySymbol property => this.GetProperty( property ),
                IFieldSymbol field => this.GetProperty( field ),
                ITypeParameterSymbol typeParameter => this.GetGenericParameter( typeParameter ),
                IParameterSymbol parameter => this.GetParameter( parameter ),
                IEventSymbol @event => this.GetEvent( @event ),
                IAssemblySymbol assembly => this.GetAssembly( assembly ),
                _ => throw new ArgumentException( nameof( symbol ) )
            };

        IArrayType ITypeFactory.MakeArrayType( IType elementType, int rank ) =>
            (IArrayType) this.GetIType( this.RoslynCompilation.CreateArrayTypeSymbol( ((ITypeInternal) elementType).TypeSymbol.AssertNotNull(), rank ) );

        IPointerType ITypeFactory.MakePointerType( IType pointedType ) =>
            (IPointerType) this.GetIType( this.RoslynCompilation.CreatePointerTypeSymbol( ((ITypeInternal) pointedType).TypeSymbol.AssertNotNull() ) );

        public IAttribute GetAttribute( AttributeBuilder attributeBuilder )
            => (IAttribute) this._cache.GetOrAdd(
                CodeElementLink.FromBuilder( attributeBuilder ),
                l => new BuiltAttribute( (AttributeBuilder) l.Target!, this._compilation ) );

        public IParameter GetParameter( ParameterBuilder parameterBuilder )
            => (IParameter) this._cache.GetOrAdd(
                CodeElementLink.FromBuilder( parameterBuilder ),
                l => new BuiltParameter( (ParameterBuilder) l.Target!, this._compilation ) );

        public IGenericParameter GetGenericParameter( GenericParameterBuilder genericParameterBuilder )
            => (IGenericParameter) this._cache.GetOrAdd(
                CodeElementLink.FromBuilder( genericParameterBuilder ),
                l => new BuiltGenericParameter( (GenericParameterBuilder) l.Target!, this._compilation ) );

        public ICodeElement GetCodeElement( CodeElementBuilder builder )
            => builder switch
            {
                MethodBuilder methodBuilder => this.GetMethod( methodBuilder ),
                ParameterBuilder parameterBuilder => this.GetParameter( parameterBuilder ),
                AttributeBuilder attributeBuilder => this.GetAttribute( attributeBuilder ),
                GenericParameterBuilder genericParameterBuilder => this.GetGenericParameter( genericParameterBuilder ),
                _ => throw new AssertionFailedException(),
            };

        public IType GetIType( IType type )
        {
            if ( type.Compilation == this._compilation )
            {
                return type;
            }
            else if ( type is ITypeInternal typeInternal )
            {
                return this.GetIType( typeInternal.TypeSymbol.AssertNotNull() );
            }
            else
            {
                // The type is necessarily backed by a Roslyn symbol because we don't support anything else.
                return this.GetIType( ((ITypeInternal) type).TypeSymbol.AssertNotNull() );
            }
        }

        public T GetCodeElement<T>( T codeElement )
            where T : ICodeElement
        {
            if ( codeElement.Compilation == this._compilation )
            {
                return codeElement;
            }
            else
            {
                var link = (ICodeElementLink<ICodeElement>) codeElement;
                return (T) link.GetForCompilation( this._compilation );
            }
        }

        public IMethod GetMethod( IMethod attributeBuilderConstructor )
        {
            throw new NotImplementedException();
        }

        public IParameter GetReturnParameter( IMethodSymbol method )
        {
            throw new NotImplementedException();
        }
    }
}