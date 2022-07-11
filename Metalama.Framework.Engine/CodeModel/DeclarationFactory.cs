// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel
{
    internal enum InternalSpecialType
    {
        ITemplateAttribute,
        Count
    }

    /// <summary>
    /// Creates instances of <see cref="IDeclaration"/> for a given <see cref="CompilationModel"/>.
    /// </summary>
    public class DeclarationFactory : IDeclarationFactory
    {
        private readonly ConcurrentDictionary<Ref<ICompilationElement>, object> _defaultCache =
            new( RefEqualityComparer<ICompilationElement>.Default );

        // For types, we have a null-sensitive comparer to that 'object' and 'object?' are cached as two distinct items.
        private readonly ConcurrentDictionary<ITypeSymbol, object> _typeCache =
            new( SymbolEqualityComparer.IncludeNullability );

        private readonly INamedType?[] _specialTypes = new INamedType?[(int) SpecialType.Count];
        private readonly INamedType?[] _internalSpecialTypes = new INamedType?[(int) InternalSpecialType.Count];

        private readonly CompilationModel _compilationModel;

        public DeclarationFactory( CompilationModel compilation )
        {
            this._compilationModel = compilation;
        }

        private Compilation RoslynCompilation => this._compilationModel.RoslynCompilation;

        public INamedType GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this._compilationModel.ReflectionMapper.GetNamedTypeSymbolByMetadataName( reflectionName, null );

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

        public IType GetTypeByReflectionType( Type type ) => this.GetIType( this._compilationModel.ReflectionMapper.GetTypeSymbol( type ) );

        internal INamespace GetNamespace( INamespaceSymbol namespaceSymbol )
        {
            if ( !StructuralSymbolComparer.Default.Equals( namespaceSymbol.ContainingAssembly, this.Compilation.Assembly ) )
            {
                throw new InvalidOperationException( "Cannot get the namespace of a type that is not a part of the current compilation." );
            }

            return (INamespace) this._defaultCache.GetOrAdd(
                namespaceSymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                l => new Namespace( (INamespaceSymbol) l.GetSymbol( this.Compilation ), this._compilationModel ) );
        }

        internal IAssembly GetAssembly( IAssemblySymbol assemblySymbol )
            => (IAssembly) this._defaultCache.GetOrAdd(
                assemblySymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                l => !((IAssemblySymbol) l.GetSymbol( this.Compilation )).Identity.Equals( this._compilationModel.RoslynCompilation.Assembly.Identity )
                    ? new ReferencedAssembly( (IAssemblySymbol) l.GetSymbol( this.Compilation ), this._compilationModel )
                    : this._compilationModel );

        public IType GetIType( ITypeSymbol typeSymbol )
            => (IType) this._typeCache.GetOrAdd(
                typeSymbol,
                l => CodeModelFactory.CreateIType( l, this._compilationModel ) );

        private IArrayType GetArrayType( IArrayTypeSymbol typeSymbol )
            => (ArrayType) this._typeCache.GetOrAdd(
                typeSymbol,
                s => new ArrayType( (IArrayTypeSymbol) s, this._compilationModel ) );

        private IDynamicType GetDynamicType( IDynamicTypeSymbol typeSymbol )
            => (DynamicType) this._typeCache.GetOrAdd(
                typeSymbol,
                s => new DynamicType( (IDynamicTypeSymbol) s, this._compilationModel ) );

        private IPointerType GetPointerType( IPointerTypeSymbol typeSymbol )
            => (PointerType) this._typeCache.GetOrAdd(
                typeSymbol,
                s => new PointerType( (IPointerTypeSymbol) s, this._compilationModel ) );

        public INamedType GetNamedType( INamedTypeSymbol typeSymbol )
        {
            if ( typeSymbol.NullableAnnotation == NullableAnnotation.Annotated )
            {
                // If we have a nullable named type, we return a nullable wrapper. We want to make sure that there is a single
                // underlying NamedType.

                return (INamedType) this._typeCache.GetOrAdd(
                    typeSymbol,
                    s =>
                        new NullableNamedType(
                            (NamedType) this.GetNamedType( (INamedTypeSymbol) s.WithNullableAnnotation( NullableAnnotation.None ) ),
                            (INamedTypeSymbol) s ) );
            }
            else
            {
                if ( typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated )
                {
                    // Normalize to the NullableAnnotation.None.
                    typeSymbol = (INamedTypeSymbol) typeSymbol.WithNullableAnnotation( NullableAnnotation.None );
                }

                return (INamedType) this._typeCache.GetOrAdd(
                    typeSymbol,
                    s => new NamedType( (INamedTypeSymbol) s, this._compilationModel ) );
            }
        }

        public ITypeParameter GetGenericParameter( ITypeParameterSymbol typeParameterSymbol )
            => (TypeParameter) this._defaultCache.GetOrAdd(
                typeParameterSymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                tp => new TypeParameter( (ITypeParameterSymbol) tp.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IMethod GetMethod( IMethodSymbol methodSymbol )
            => (IMethod) this._defaultCache.GetOrAdd(
                methodSymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Method( (IMethodSymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IProperty GetProperty( IPropertySymbol propertySymbol )
            => (IProperty) this._defaultCache.GetOrAdd(
                propertySymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Property( (IPropertySymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IIndexer GetIndexer( IPropertySymbol propertySymbol )
            => (IIndexer) this._defaultCache.GetOrAdd(
                propertySymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Indexer( (IPropertySymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IField GetField( IFieldSymbol fieldSymbol )
            => (IField) this._defaultCache.GetOrAdd(
                fieldSymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Field( (IFieldSymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IConstructor GetConstructor( IMethodSymbol methodSymbol )
            => (IConstructor) this._defaultCache.GetOrAdd(
                methodSymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Constructor( (IMethodSymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IMethod GetFinalizer( IMethodSymbol finalizerSymbol )
            => (IMethod) this._defaultCache.GetOrAdd(
                finalizerSymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Method( (IMethodSymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IParameter GetParameter( IParameterSymbol parameterSymbol )
            => (IParameter) this._defaultCache.GetOrAdd(
                parameterSymbol.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Parameter( (IParameterSymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        public IEvent GetEvent( IEventSymbol @event )
            => (IEvent) this._defaultCache.GetOrAdd(
                @event.ToTypedRef( this.Compilation ).As<ICompilationElement>(),
                ms => new Event( (IEventSymbol) ms.GetSymbol( this.Compilation ), this._compilationModel ) );

        internal bool TryGetDeclaration( ISymbol symbol, [NotNullWhen( true )] out IDeclaration? declaration )
        {
            var compilationElement = this.GetCompilationElement( symbol );
            declaration = compilationElement as IDeclaration;

            return declaration != null;
        }

        internal IDeclaration? GetDeclarationOrNull( ISymbol symbol, DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
            => this.GetCompilationElement( symbol, kind ) as IDeclaration;

        public IDeclaration GetDeclaration( SymbolDictionaryKey key ) => this.GetDeclaration( key.GetId().Resolve( this.Compilation ).AssertNotNull() );

        public IDeclaration GetDeclaration( ISymbol symbol ) => this.GetDeclaration( symbol, DeclarationRefTargetKind.Default );

        internal IDeclaration GetDeclaration( ISymbol symbol, DeclarationRefTargetKind kind )
        {
            var compilationElement = this.GetCompilationElement( symbol, kind );

            if ( compilationElement is not IDeclaration declaration )
            {
                throw new ArgumentException( nameof(symbol), $"{symbol.Kind} is not a declaration" );
            }

            return declaration;
        }

        internal ICompilationElement? GetCompilationElement( ISymbol symbol, DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
        {
            Invariant.Assert(
                FixedSymbolComparer.Default.Equals( symbol, symbol.GetSymbolId().Resolve( this._compilationModel.RoslynCompilation ).AssertNotNull() ) );

            switch ( symbol.Kind )
            {
                case SymbolKind.NamedType:
                    var type = this.GetNamedType( (INamedTypeSymbol) symbol );

                    return kind switch
                    {
                        DeclarationRefTargetKind.StaticConstructor => type.StaticConstructor,
                        DeclarationRefTargetKind.Default => type,
                        _ => throw new AssertionFailedException()
                    };

                case SymbolKind.ArrayType:
                    return this.GetArrayType( (IArrayTypeSymbol) symbol );

                case SymbolKind.PointerType:
                    return this.GetPointerType( (IPointerTypeSymbol) symbol );

                case SymbolKind.DynamicType:
                    return this.GetDynamicType( (IDynamicTypeSymbol) symbol );

                case SymbolKind.Method:
                    {
                        var method = (IMethodSymbol) symbol;

                        return
                            kind == DeclarationRefTargetKind.Return
                                ? this.GetReturnParameter( method )
                                : method.GetDeclarationKind() switch
                                {
                                    DeclarationKind.Method => this.GetMethod( method ),
                                    DeclarationKind.Constructor => this.GetConstructor( method ),
                                    DeclarationKind.Finalizer => this.GetFinalizer( method ),
                                    _ => throw new AssertionFailedException()
                                };
                    }

                case SymbolKind.Property:
                    var propertySymbol = (IPropertySymbol) symbol;

                    var propertyOrIndexer = propertySymbol.IsIndexer
                        ? (IPropertyOrIndexer) this.GetIndexer( propertySymbol )
                        : this.GetProperty( propertySymbol );

                    return kind switch
                    {
                        // Implicit getter or setter.
                        DeclarationRefTargetKind.PropertyGet => propertyOrIndexer.GetMethod,
                        DeclarationRefTargetKind.PropertySet => propertyOrIndexer.SetMethod,

                        // The property itself.
                        DeclarationRefTargetKind.Default => propertyOrIndexer,
                        _ => throw new AssertionFailedException()
                    };

                case SymbolKind.Field:
                    return this.GetField( (IFieldSymbol) symbol );

                case SymbolKind.TypeParameter:
                    return this.GetGenericParameter( (ITypeParameterSymbol) symbol );

                case SymbolKind.Parameter:
                    return this.GetParameter( (IParameterSymbol) symbol );

                case SymbolKind.Event:
                    return this.GetEvent( (IEventSymbol) symbol );

                case SymbolKind.Assembly:
                    return this.GetAssembly( (IAssemblySymbol) symbol );

                case SymbolKind.Namespace:
                    return this.GetNamespace( (INamespaceSymbol) symbol );

                case SymbolKind.NetModule:
                    return this._compilationModel;

                case SymbolKind.Local:
                case SymbolKind.Label:
                case SymbolKind.ErrorType:
                    return null;

                default:
                    throw new AssertionFailedException( $"Don't know how to resolve a '{symbol.Kind}'." );
            }
        }

        IArrayType IDeclarationFactory.ConstructArrayType( IType elementType, int rank )
            => (IArrayType) this.GetIType( this.RoslynCompilation.CreateArrayTypeSymbol( ((ITypeInternal) elementType).TypeSymbol.AssertNotNull(), rank ) );

        IPointerType IDeclarationFactory.ConstructPointerType( IType pointedType )
            => (IPointerType) this.GetIType( this.RoslynCompilation.CreatePointerTypeSymbol( ((ITypeInternal) pointedType).TypeSymbol.AssertNotNull() ) );

        public T ConstructNullable<T>( T type )
            where T : IType
            => (T) this.GetIType( ((ITypeInternal) type).TypeSymbol.AssertNotNull().WithNullableAnnotation( NullableAnnotation.Annotated ) );

        public INamedType GetSpecialType( SpecialType specialType ) => this._specialTypes[(int) specialType] ??= this.GetSpecialTypeCore( specialType );

        internal INamedType GetSpecialType( InternalSpecialType specialType )
            => this._internalSpecialTypes[(int) specialType] ??= this.GetSpecialTypeCore( specialType );

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

        private INamedType GetSpecialTypeCore( InternalSpecialType specialType )
            => specialType switch
            {
                InternalSpecialType.ITemplateAttribute => (INamedType) this.GetTypeByReflectionType( typeof(ITemplateAttribute) ),
                _ => throw new ArgumentOutOfRangeException( nameof(specialType) )
            };

        object? IDeclarationFactory.DefaultValue( IType type ) => new DefaultUserExpression( type );

        object? IDeclarationFactory.Cast( IType type, object? value ) => new CastUserExpression( type, value );

        public IDeclaration GetDeclarationFromSymbolId( SymbolId symbolId )
            => this.GetDeclaration( symbolId.Resolve( this.RoslynCompilation ).AssertNotNull() );

        public IDeclaration GetDeclarationFromSerializableId( DeclarationSerializableId declarationId )
        {
            var symbol = Ref<IDeclaration>.Deserialize( this.Compilation, declarationId.Id );

            if ( symbol == null )
            {
                throw new InvalidOperationException(
                    $"Cannot find the symbol '{declarationId}' in compilation '{this._compilationModel.RoslynCompilation.Assembly.Name}'." );
            }

            return this.GetDeclaration( symbol );
        }

        public IDeclaration Translate( IDeclaration declaration, ReferenceResolutionOptions options = default )
        {
            if ( ReferenceEquals( declaration.Compilation, this._compilationModel ) )
            {
                return declaration;
            }
            else
            {
                return declaration.ToTypedRef().GetTarget( this._compilationModel, options );
            }
        }

        internal IAttribute GetAttribute( AttributeBuilder attributeBuilder, ReferenceResolutionOptions options )
            => (IAttribute) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( attributeBuilder ).As<ICompilationElement>(),
                l => new BuiltAttribute( (AttributeBuilder) l.Target!, this._compilationModel ) );

        private static Exception CreateBuilderNotExists( IDeclarationBuilder builder )
            => new InvalidOperationException( $"The declaration '{builder}' does not exist in the current compilation." );

        internal IParameter GetParameter( ParameterBuilder parameterBuilder, ReferenceResolutionOptions options )
        {
            if ( options.MustExist() && !this._compilationModel.Contains( parameterBuilder ) )
            {
                throw CreateBuilderNotExists( parameterBuilder );
            }

            return (IParameter) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( parameterBuilder ).As<ICompilationElement>(),
                l => new BuiltParameter( (IParameterBuilder) l.Target!, this._compilationModel ) );
        }

        internal ITypeParameter GetGenericParameter( TypeParameterBuilder typeParameterBuilder, ReferenceResolutionOptions options )
            => (ITypeParameter) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( typeParameterBuilder ).As<ICompilationElement>(),
                l => new BuiltTypeParameter( (TypeParameterBuilder) l.Target!, this._compilationModel ) );

        internal IMethod GetMethod( MethodBuilder methodBuilder, ReferenceResolutionOptions options )
        {
            if ( options.MustExist() && !this._compilationModel.Contains( methodBuilder ) )
            {
                throw CreateBuilderNotExists( methodBuilder );
            }

            return (IMethod) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( methodBuilder ).As<ICompilationElement>(),
                l => new BuiltMethod( (MethodBuilder) l.Target!, this._compilationModel ) );
        }

        internal IMethod GetAccessor( AccessorBuilder methodBuilder, ReferenceResolutionOptions options )
        {
            return (IMethod) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( methodBuilder ).As<ICompilationElement>(),
                l =>
                {
                    var builder = (AccessorBuilder) l.Target!;

                    return ((IMemberWithAccessors) this.GetDeclaration<IMember>( builder.ContainingMember, options )).GetAccessor( builder.MethodKind )!;
                } );
        }

        internal IConstructor GetConstructor( ConstructorBuilder methodBuilder, ReferenceResolutionOptions options )
        {
            if ( options.MustExist() && !this._compilationModel.Contains( methodBuilder ) )
            {
                throw CreateBuilderNotExists( methodBuilder );
            }

            return (IConstructor) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( methodBuilder ).As<ICompilationElement>(),
                l => new BuiltConstructor( (ConstructorBuilder) l.Target!, this._compilationModel ) );
        }

        internal IField GetField( FieldBuilder fieldBuilder, ReferenceResolutionOptions options )
        {
            if ( options.MustExist() && !this._compilationModel.Contains( fieldBuilder ) )
            {
                throw CreateBuilderNotExists( fieldBuilder );
            }

            return (IField) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( fieldBuilder ).As<ICompilationElement>(),
                l => new BuiltField( (FieldBuilder) l.Target!, this._compilationModel ) );
        }

        internal IProperty GetProperty( PropertyBuilder propertyBuilder, ReferenceResolutionOptions options )
        {
            if ( options.MustExist() && !this._compilationModel.Contains( propertyBuilder ) )
            {
                throw CreateBuilderNotExists( propertyBuilder );
            }

            return (IProperty) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( propertyBuilder ).As<ICompilationElement>(),
                l => new BuiltProperty( (PropertyBuilder) l.Target!, this._compilationModel ) );
        }

        internal IEvent GetEvent( EventBuilder propertyBuilder, ReferenceResolutionOptions options )
        {
            if ( options.MustExist() && !this._compilationModel.Contains( propertyBuilder ) )
            {
                throw CreateBuilderNotExists( propertyBuilder );
            }

            return (IEvent) this._defaultCache.GetOrAdd(
                Ref.FromBuilder( propertyBuilder ).As<ICompilationElement>(),
                l => new BuiltEvent( (EventBuilder) l.Target!, this._compilationModel ) );
        }

        internal IDeclaration GetDeclaration( IDeclarationBuilder builder, ReferenceResolutionOptions options = default )
            => builder switch
            {
                MethodBuilder methodBuilder => this.GetMethod( methodBuilder, options ),
                FieldBuilder fieldBuilder => this.GetField( fieldBuilder, options ),
                PropertyBuilder propertyBuilder => this.GetProperty( propertyBuilder, options ),
                EventBuilder eventBuilder => this.GetEvent( eventBuilder, options ),
                ParameterBuilder parameterBuilder => this.GetParameter( parameterBuilder, options ),
                AttributeBuilder attributeBuilder => this.GetAttribute( attributeBuilder, options ),
                TypeParameterBuilder genericParameterBuilder => this.GetGenericParameter( genericParameterBuilder, options ),
                AccessorBuilder accessorBuilder => this.GetAccessor( accessorBuilder, options ),
                ConstructorBuilder constructorBuilder => this.GetConstructor( constructorBuilder, options ),

                // This is for linker tests (fake builders), which resolve to themselves.
                // ReSharper disable once SuspiciousTypeConversion.Global
                ISdkRef<IDeclaration> reference => reference.GetTarget( this._compilationModel ).AssertNotNull(),
                _ => throw new AssertionFailedException()
            };

        public IType GetIType( IType type )
        {
            if ( type.Compilation == this._compilationModel )
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
        public T? GetDeclaration<T>( T? declaration, ReferenceResolutionOptions options = default )
            where T : class, IDeclaration
        {
            if ( declaration == null )
            {
                return null;
            }

            if ( declaration.Compilation == this._compilationModel )
            {
                return declaration;
            }
            else if ( declaration is ISdkRef<IDeclaration> reference )
            {
                return (T) reference.GetTarget( this._compilationModel ).AssertNotNull();
            }
            else if ( declaration is NamedType namedType )
            {
                // TODO: This would not work after type introductions, but that would require more changes.
                return (T) this.GetNamedType( (INamedTypeSymbol) namedType.Symbol );
            }
            else
            {
                return declaration.ToTypedRef().GetTarget( this._compilationModel, options );
            }
        }

        public IConstructor GetConstructor( IConstructor attributeBuilderConstructor ) => this.GetDeclaration( attributeBuilderConstructor );

        public IParameter GetReturnParameter( IMethodSymbol methodSymbol ) => this.GetMethod( methodSymbol ).ReturnParameter;

        private Compilation Compilation => this._compilationModel.RoslynCompilation;

        public Type GetReflectionType( ITypeSymbol typeSymbol )
            => this._compilationModel.Project.ServiceProvider.GetRequiredService<SystemTypeResolver>().GetCompileTimeType( typeSymbol, true ).AssertNotNull();
    }
}