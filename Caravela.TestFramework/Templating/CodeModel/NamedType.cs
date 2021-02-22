using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Microsoft.CodeAnalysis;
using Accessibility = Caravela.Framework.Code.Accessibility;
using CompilationModel = Caravela.Framework.Impl.CodeModel.Symbolic.CompilationModel;
using ITypeInternal = Caravela.Framework.Impl.CodeModel.Symbolic.ITypeInternal;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.TestFramework.Templating.CodeModel
{
    internal class NamedType : CodeElement, INamedType, ITypeInternal
    {
        private readonly INamedTypeSymbol _symbol;

        public NamedType( INamedTypeSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        protected internal override ISymbol Symbol => this._symbol;

        public TypeKind TypeKind => throw new NotImplementedException();

        public bool HasDefaultConstructor => throw new NotImplementedException();

        public INamedType? BaseType => throw new NotImplementedException();

        public IReadOnlyList<INamedType> ImplementedInterfaces => throw new NotImplementedException();

        public string Name => this._symbol.Name;

        public string? Namespace => throw new NotImplementedException();

        public string FullName => throw new NotImplementedException();

        public IReadOnlyList<IType> GenericArguments => throw new NotImplementedException();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public bool IsOpenGeneric => throw new NotImplementedException();

        public IReadOnlyList<INamedType> NestedTypes => throw new NotImplementedException();

        public IReadOnlyList<IProperty> Properties => throw new NotImplementedException();

        public IReadOnlyList<IEvent> Events => throw new NotImplementedException();

        public IReadOnlyList<IMethod> Methods => throw new NotImplementedException();

        public override CodeElementKind ElementKind => CodeElementKind.Type;

        public ITypeSymbol TypeSymbol => this._symbol;

        public bool IsPartial => throw new NotImplementedException();

        public IReadOnlyList<IConstructor> Constructors => throw new NotImplementedException();

        public IConstructor? StaticConstructor => throw new NotImplementedException();

        public ITypeFactory TypeFactory => new TypeFactoryImpl( this.Compilation );

        public Accessibility Accessibility => throw new NotImplementedException();

        public bool IsAbstract => throw new NotImplementedException();

        public bool IsStatic => throw new NotImplementedException();

        public bool IsVirtual => throw new NotImplementedException();

        public bool IsSealed => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsOverride => throw new NotImplementedException();

        public bool IsNew => throw new NotImplementedException();

        public bool IsAsync => throw new NotImplementedException();

        public INamedType DeclaringType => throw new NotImplementedException();

        public IArrayType MakeArrayType( int rank = 1 ) => throw new NotImplementedException();

        public IPointerType MakePointerType() => throw new NotImplementedException();

        public INamedType WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        private class TypeFactoryImpl : ITypeFactory
        {
            private readonly CompilationModel _compilation;

            public TypeFactoryImpl( CompilationModel compilation )
            {
                this._compilation = compilation;
            }

            public INamedType GetTypeByReflectionName( string reflectionName ) => throw new NotImplementedException();

            public IType GetTypeByReflectionType( Type type ) => throw new NotImplementedException();

            public bool Is( IType left, IType right ) => this._compilation.RoslynCompilation.HasImplicitConversion( left.GetSymbol(), right.GetSymbol() );

            public bool Is( IType left, Type right ) =>
                this.Is( left, this._compilation.Factory.GetTypeByReflectionType( right ) ?? throw new ArgumentException( $"Could not resolve type {right}.", nameof( right ) ) );

            public IArrayType MakeArrayType( IType elementType, int rank ) => throw new NotImplementedException();

            public IPointerType MakePointerType( IType pointedType ) => throw new NotImplementedException();
        }
    }
}
