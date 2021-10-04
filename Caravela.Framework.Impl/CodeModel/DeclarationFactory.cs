// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Code.Types;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Creates instances of <see cref="IDeclaration"/> for a given <see cref="CompilationModel"/>.
    /// </summary>
    internal class DeclarationFactory : ITypeFactory
    {
        private readonly ConcurrentDictionary<DeclarationRef<IDeclaration>, object> _cache =
            new( DeclarationRefEqualityComparer<DeclarationRef<IDeclaration>>.Instance );

        private readonly INamedType?[] _specialTypes = new INamedType?[(int) SpecialType.Count];

        public DeclarationFactory( CompilationModel compilation )
        {
            this.CompilationModel = compilation;
        }

        public CompilationModel CompilationModel { get; }

        public SyntaxSerializationService Serializers { get; } = new();

        private Compilation RoslynCompilation => this.CompilationModel.RoslynCompilation;

        public INamedType GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this.CompilationModel.ReflectionMapper.GetNamedTypeSymbolByMetadataName( reflectionName );

            return this.GetNamedType( symbol );
        }

        public bool TryGetTypeByReflectionName( string reflectionName, [NotNullWhen( true )] out INamedType? namedType )
        {
            var symbol = this.Compilation.GetTypeByMetadataName( reflectionName );

            if ( symbol == null )
            {
                namedType = null;

                return false;
            }
            else
            {
                namedType = this.GetNamedType( symbol );

                return true;
            }
        }

        public IType GetTypeByReflectionType( Type type ) => this.GetIType( this.CompilationModel.ReflectionMapper.GetTypeSymbol( type ) );

        internal INamespace GetNamespace( INamespaceSymbol namespaceSymbol )
            => (INamespace) this._cache.GetOrAdd(
                namespaceSymbol.ToRef(),
                l => new Namespace( (INamespaceSymbol) l.GetSymbol( this.Compilation ), this.CompilationModel ) );

        internal IAssembly GetAssembly( IAssemblySymbol assemblySymbol )
            => (IAssembly) this._cache.GetOrAdd(
                assemblySymbol.ToRef(),
                l => !SymbolEqualityComparer.Default.Equals( l.GetSymbol( this.Compilation ), this.CompilationModel.RoslynCompilation.Assembly )
                    ? new ReferencedAssembly( (IAssemblySymbol) l.GetSymbol( this.Compilation ), this.CompilationModel )
                    : this.CompilationModel );

        public IType GetIType( ITypeSymbol typeSymbol )
            => (IType) this._cache.GetOrAdd(
                typeSymbol.ToRef(),
                l => CodeModelFactory.CreateIType( (ITypeSymbol) l.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public INamedType GetNamedType( INamedTypeSymbol typeSymbol )
            => (NamedType) this._cache.GetOrAdd(
                typeSymbol.ToRef(),
                s => new NamedType( (INamedTypeSymbol) s.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public IGenericParameter GetGenericParameter( ITypeParameterSymbol typeParameterSymbol )
            => (GenericParameter) this._cache.GetOrAdd(
                typeParameterSymbol.ToRef(),
                tp => new GenericParameter( (ITypeParameterSymbol) tp.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public IMethod GetMethod( IMethodSymbol methodSymbol )
            => (IMethod) this._cache.GetOrAdd(
                methodSymbol.ToRef(),
                ms => new Method( (IMethodSymbol) ms.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public IProperty GetProperty( IPropertySymbol propertySymbol )
            => (IProperty) this._cache.GetOrAdd(
                propertySymbol.ToRef(),
                ms => new Property( (IPropertySymbol) ms.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public IField GetField( IFieldSymbol fieldSymbol )
            => (IField) this._cache.GetOrAdd( fieldSymbol.ToRef(), ms => new Field( (IFieldSymbol) ms.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public IConstructor GetConstructor( IMethodSymbol methodSymbol )
            => (IConstructor) this._cache.GetOrAdd(
                methodSymbol.ToRef(),
                ms => new Constructor( (IMethodSymbol) ms.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public IParameter GetParameter( IParameterSymbol parameterSymbol )
            => (IParameter) this._cache.GetOrAdd(
                parameterSymbol.ToRef(),
                ms => new Parameter( (IParameterSymbol) ms.GetSymbol( this.Compilation ), this.CompilationModel ) );

        public IEvent GetEvent( IEventSymbol @event )
            => (IEvent) this._cache.GetOrAdd( @event.ToRef(), ms => new Event( (IEventSymbol) ms.GetSymbol( this.Compilation ), this.CompilationModel ) );

        internal IDeclaration GetDeclaration( ISymbol? symbol, DeclarationSpecialKind kind = DeclarationSpecialKind.Default )
            => symbol switch
            {
                INamespaceSymbol => this.CompilationModel,
                INamedTypeSymbol namedType => this.GetNamedType( namedType ),
                IMethodSymbol method =>
                    kind == DeclarationSpecialKind.ReturnParameter
                        ? this.GetReturnParameter( method )
                        : method.GetDeclarationKind() == DeclarationKind.Method
                            ? this.GetMethod( method )
                            : this.GetConstructor( method ),
                IPropertySymbol property => this.GetProperty( property ),
                IFieldSymbol field => this.GetField( field ),
                ITypeParameterSymbol typeParameter => this.GetGenericParameter( typeParameter ),
                IParameterSymbol parameter => this.GetParameter( parameter ),
                IEventSymbol @event => this.GetEvent( @event ),
                IAssemblySymbol assembly => this.GetAssembly( assembly ),
                _ => throw new ArgumentException( nameof(symbol) )
            };

        IArrayType ITypeFactory.ConstructArrayType( IType elementType, int rank )
            => (IArrayType) this.GetIType( this.RoslynCompilation.CreateArrayTypeSymbol( ((ITypeInternal) elementType).TypeSymbol.AssertNotNull(), rank ) );

        IPointerType ITypeFactory.ConstructPointerType( IType pointedType )
            => (IPointerType) this.GetIType( this.RoslynCompilation.CreatePointerTypeSymbol( ((ITypeInternal) pointedType).TypeSymbol.AssertNotNull() ) );

        public T ConstructNullable<T>( T type )
            where T : IType
            => (T) this.GetIType( ((ITypeInternal) type).TypeSymbol.AssertNotNull().WithNullableAnnotation( NullableAnnotation.Annotated ) );

        public INamedType GetSpecialType( SpecialType specialType ) => this._specialTypes[(int) specialType] ??= this.GetSpecialTypeCore( specialType );

        private INamedType GetSpecialTypeCore( SpecialType specialType )
        {
            var roslynSpecialType = specialType.ToRoslynSpecialType();

            if ( roslynSpecialType != Microsoft.CodeAnalysis.SpecialType.None )
            {
                return this.GetNamedType( this.RoslynCompilation.GetSpecialType( roslynSpecialType ) );
            }
            else
            {
                return
                    specialType switch
                    {
                        SpecialType.List_T => (INamedType) this.GetTypeByReflectionType( typeof(List<>) ),
                        SpecialType.ValueTask => (INamedType) this.GetTypeByReflectionType( typeof(ValueTask) ),
                        SpecialType.ValueTask_T => (INamedType) this.GetTypeByReflectionType( typeof(ValueTask<>) ),
                        SpecialType.Task => (INamedType) this.GetTypeByReflectionType( typeof(Task) ),
                        SpecialType.Task_T => (INamedType) this.GetTypeByReflectionType( typeof(Task<>) ),
                        SpecialType.IAsyncEnumerable_T => this.GetTypeByReflectionName( "System.Collections.Generic.IAsyncEnumerable`1" ),
                        SpecialType.IAsyncEnumerator_T => this.GetTypeByReflectionName( "System.Collections.Generic.IAsyncEnumerator`1" ),
                        _ => throw new ArgumentOutOfRangeException( nameof(specialType) )
                    };
            }
        }

        object? ITypeFactory.DefaultValue( IType type ) => new DefaultUserExpression( type );

        object? ITypeFactory.Cast( IType type, object? value ) => new CastUserExpression( type, value );

        internal IAttribute GetAttribute( AttributeBuilder attributeBuilder )
            => (IAttribute) this._cache.GetOrAdd(
                DeclarationRef.FromBuilder( attributeBuilder ),
                l => new BuiltAttribute( (AttributeBuilder) l.Target!, this.CompilationModel ) );

        internal IParameter GetParameter( ParameterBuilder parameterBuilder )
            => (IParameter) this._cache.GetOrAdd(
                DeclarationRef.FromBuilder( parameterBuilder ),
                l => new BuiltParameter( (ParameterBuilder) l.Target!, this.CompilationModel ) );

        internal IGenericParameter GetGenericParameter( GenericParameterBuilder genericParameterBuilder )
            => (IGenericParameter) this._cache.GetOrAdd(
                DeclarationRef.FromBuilder( genericParameterBuilder ),
                l => new BuiltGenericParameter( (GenericParameterBuilder) l.Target!, this.CompilationModel ) );

        internal IMethod GetMethod( MethodBuilder methodBuilder )
            => (IMethod) this._cache.GetOrAdd(
                DeclarationRef.FromBuilder( methodBuilder ),
                l => new BuiltMethod( (MethodBuilder) l.Target!, this.CompilationModel ) );

        internal IProperty GetProperty( PropertyBuilder propertyBuilder )
            => (IProperty) this._cache.GetOrAdd(
                DeclarationRef.FromBuilder( propertyBuilder ),
                l => new BuiltProperty( (PropertyBuilder) l.Target!, this.CompilationModel ) );

        internal IEvent GetEvent( EventBuilder propertyBuilder )
            => (IEvent) this._cache.GetOrAdd(
                DeclarationRef.FromBuilder( propertyBuilder ),
                l => new BuiltEvent( (EventBuilder) l.Target!, this.CompilationModel ) );

        internal IDeclaration GetDeclaration( IDeclarationBuilder builder )
            => builder switch
            {
                MethodBuilder methodBuilder => this.GetMethod( methodBuilder ),
                PropertyBuilder propertyBuilder => this.GetProperty( propertyBuilder ),
                EventBuilder eventBuilder => this.GetEvent( eventBuilder ),
                ParameterBuilder parameterBuilder => this.GetParameter( parameterBuilder ),
                AttributeBuilder attributeBuilder => this.GetAttribute( attributeBuilder ),
                GenericParameterBuilder genericParameterBuilder => this.GetGenericParameter( genericParameterBuilder ),

                // This is for linker tests (fake builders), which resolve to themselves.
                // ReSharper disable once SuspiciousTypeConversion.Global
                IDeclarationRef<IDeclaration> reference => reference.Resolve( this.CompilationModel ).AssertNotNull(),
                _ => throw new AssertionFailedException()
            };

        public IType GetIType( IType type )
        {
            if ( type.Compilation == this.CompilationModel )
            {
                return type;
            }

            if ( type is ITypeInternal typeInternal )
            {
                return this.GetIType( typeInternal.TypeSymbol.AssertNotNull() );
            }

            // The type is necessarily backed by a Roslyn symbol because we don't support anything else.
            return this.GetIType( ((ITypeInternal) type).TypeSymbol.AssertNotNull() );
        }

        [return: NotNullIfNotNull( "declaration" )]
        public T? GetDeclaration<T>( T? declaration )
            where T : class, IDeclaration
        {
            if ( declaration == null )
            {
                return null;
            }

            if ( declaration.Compilation == this.CompilationModel )
            {
                return declaration;
            }
            else if ( declaration is IDeclarationRef<IDeclaration> reference )
            {
                return (T) reference.Resolve( this.CompilationModel ).AssertNotNull();
            }
            else if ( declaration is NamedType namedType )
            {
                // TODO: This would not work after type introductions, but that would require more changes.
                return (T) this.GetNamedType( (INamedTypeSymbol) namedType.Symbol );
            }
            else
            {
                return declaration.ToRef().Resolve( this.CompilationModel );
            }
        }

        public IConstructor GetConstructor( IConstructor attributeBuilderConstructor ) => this.GetDeclaration( attributeBuilderConstructor );

        public IParameter GetReturnParameter( IMethodSymbol methodSymbol ) => this.GetMethod( methodSymbol ).ReturnParameter;

        private Compilation Compilation => this.CompilationModel.RoslynCompilation;
    }
}