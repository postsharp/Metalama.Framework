// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.CompileTime.Serialization.Serializers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// Creates instances of <see cref="IDeclaration"/> for a given <see cref="CompilationModel"/>.
/// </summary>
[PublicAPI]
public sealed partial class DeclarationFactory : IDeclarationFactory, ISdkDeclarationFactory
{
    private readonly ConcurrentDictionary<AttributeSerializationData, DeserializedAttribute> _deserializedAttributes = new();

    private readonly INamedType?[] _specialTypes = new INamedType?[(int) SpecialType.Count];
    private readonly INamedType?[] _internalSpecialTypes = new INamedType?[(int) InternalSpecialType.Count];

    private readonly CompilationModel _compilationModel;
    private readonly CompileTimeTypeResolver _systemTypeResolver;

    internal DeclarationFactory( CompilationModel compilation )
    {
        this._compilationModel = compilation;
        this._builderCache = new Cache<IDeclarationBuilder, IDeclaration>( ReferenceEqualityComparer<IDeclarationBuilder>.Instance );
        this._symbolCache = new Cache<ISymbol, IDeclaration>( compilation.CompilationContext.SymbolComparer );
        this._typeCache = new Cache<ITypeSymbol, IType>( compilation.CompilationContext.SymbolComparerIncludingNullability );

        this._systemTypeResolver = compilation.Project.ServiceProvider.GetRequiredService<SystemTypeResolver.Provider>()
            .Get( compilation.CompilationContext );
    }

    private Compilation RoslynCompilation => this._compilationModel.RoslynCompilation;

    public INamedType GetTypeByReflectionName( string reflectionName )
    {
        var symbol = this._compilationModel.CompilationContext.ReflectionMapper.GetNamedTypeSymbolByMetadataName( reflectionName, null );

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

    public IType GetTypeByReflectionType( Type type ) => this.GetIType( this._compilationModel.CompilationContext.ReflectionMapper.GetTypeSymbol( type ) );

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
                    SpecialType.Type => (INamedType) this.GetTypeByReflectionType( typeof(Type) ),
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

    object IDeclarationFactory.Cast( IType type, object? value ) => new CastUserExpression( type, value );

    public IDeclaration GetDeclarationFromId( SerializableDeclarationId declarationId )
    {
        var declaration =
            declarationId.ResolveToDeclaration( this._compilationModel )
            ?? throw new InvalidOperationException(
                $"Cannot find the symbol '{declarationId}' in compilation '{this._compilationModel.RoslynCompilation.Assembly.Name}'." );

        return (IDeclaration) declaration;
    }

    public T? Translate<T>(
        T? compilationElement,
        IGenericContext? genericContext = null )
        where T : class, ICompilationElement
        => (T?) ((ICompilationElementImpl?) compilationElement)?.Translate( this._compilationModel, genericContext, typeof(T) );

    public IType GetTypeFromId( SerializableTypeId serializableTypeId, IReadOnlyDictionary<string, IType>? genericArguments )
        => this._compilationModel.SerializableTypeIdResolver.ResolveId( serializableTypeId, genericArguments );

    private Compilation Compilation => this._compilationModel.RoslynCompilation;

    private CompilationContext CompilationContext => this._compilationModel.CompilationContext;

    public Type GetReflectionType( ITypeSymbol typeSymbol ) => this._systemTypeResolver.GetCompileTimeType( typeSymbol, true ).AssertNotNull();

    internal DeserializedAttribute GetDeserializedAttribute( AttributeSerializationData serializationData )
        => this._deserializedAttributes.GetOrAdd(
            serializationData,
            static ( data, compilation ) => new DeserializedAttribute( data, compilation ),
            this._compilationModel );

    public void Invalidate( IDeclaration declaration )
    {
        switch ( declaration )
        {
            case SymbolBasedDeclaration symbolBasedDeclaration:
                this._symbolCache.Remove( symbolBasedDeclaration.Symbol.AssertSymbolNotNull() );

                break;

            case IDeclarationBuilder declarationBuilder:
                this._builderCache.Remove( declarationBuilder );

                break;

            case BuiltDeclaration builtDeclaration:
                this._builderCache.Remove( builtDeclaration.Builder );

                break;

            default:
                throw new AssertionFailedException();
        }
    }

    internal IField? GetField( FieldBuilder fieldBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( fieldBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( fieldBuilder );
            }
            else
            {
                return null;
            }
        }

        return (IField) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( fieldBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltField( c, (FieldBuilder) l.Target! ),
            this._compilationModel );
    }

    internal IFieldOrProperty? GetProperty( PropertyBuilder propertyBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        /*
        if ( propertyBuilder is PromotedField promotedField )
        {
            // When getting a promoted field, we need to look at the current CompilationModel. Are we before or after
            // promotion? The result will be different


            return promotedField.Field switch
            {
                BuiltField builtField => this.GetField( builtField.FieldBuilder, options ),
                FieldBuilder fieldBuilder => this.GetField( fieldBuilder, options ),
                Field field => this.GetField( field.GetSymbol().AssertNotNull( ) ),
                _ => throw new AssertionFailedException()
            };
        }
        */

        if ( options.MustExist() && !this._compilationModel.Contains( propertyBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( propertyBuilder );
            }
            else
            {
                return null;
            }
        }

        return (IProperty) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( propertyBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltProperty( c, (PropertyBuilder) l.Target! ),
            this._compilationModel );
    }

    internal IIndexer? GetIndexer( IndexerBuilder indexerBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( indexerBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( indexerBuilder );
            }
            else
            {
                return null;
            }
        }

        return (IIndexer) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( indexerBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltIndexer( c, (IndexerBuilder) l.Target! ),
            this._compilationModel );
    }

    internal IEvent? GetEvent( EventBuilder eventBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( eventBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( eventBuilder );
            }
            else
            {
                return null;
            }
        }

        return (IEvent) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( eventBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltEvent( c, (EventBuilder) l.Target! ),
            this._compilationModel );
    }

    internal INamedType? GetNamedType( NamedTypeBuilder namedTypeBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( namedTypeBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( namedTypeBuilder );
            }
            else
            {
                return null;
            }
        }

        return (INamedType) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( namedTypeBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltNamedType( c, (NamedTypeBuilder) l.Target! ),
            this._compilationModel );
    }

    internal INamespace? GetNamespace( NamespaceBuilder namespaceBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( namespaceBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( namespaceBuilder );
            }
            else
            {
                return null;
            }
        }

        return (INamespace) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( namespaceBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltNamespace( c, (NamespaceBuilder) l.Target! ),
            this._compilationModel );
    }

    internal IDeclaration? GetDeclaration( IDeclarationBuilder builder, ReferenceResolutionOptions options = default, bool throwIfMissing = true )
        => builder switch
        {
            MethodBuilder methodBuilder => this.GetMethod( methodBuilder, options, throwIfMissing ),
            FieldBuilder fieldBuilder => this.GetField( fieldBuilder, options, throwIfMissing ),
            PropertyBuilder propertyBuilder => this.GetProperty( propertyBuilder, options, throwIfMissing ),
            IndexerBuilder indexerBuilder => this.GetIndexer( indexerBuilder, options, throwIfMissing ),
            EventBuilder eventBuilder => this.GetEvent( eventBuilder, options, throwIfMissing ),
            BaseParameterBuilder parameterBuilder => this.GetParameter( parameterBuilder, options, throwIfMissing ),
            AttributeBuilder attributeBuilder => this.GetAttribute( attributeBuilder, options ),
            TypeParameterBuilder genericParameterBuilder => this.GetGenericParameter( genericParameterBuilder, options ),
            AccessorBuilder accessorBuilder => this.GetAccessor( accessorBuilder, options ),
            ConstructorBuilder constructorBuilder => this.GetConstructor( constructorBuilder, options, throwIfMissing ),
            NamedTypeBuilder namedTypeBuilder => this.GetNamedType( namedTypeBuilder, options, throwIfMissing ),
            NamespaceBuilder namespaceBuilder => this.GetNamespace( namespaceBuilder, options, throwIfMissing ),

            // This is for linker tests (fake builders), which resolve to themselves.
            // ReSharper disable once SuspiciousTypeConversion.Global
            ISdkRef<IDeclaration> reference => reference.GetTarget( this._compilationModel ).AssertNotNull(),
            _ => throw new AssertionFailedException( $"Cannot get a declaration for a {builder.GetType()}" )
        };

    internal IDeclaration GetDeclaration( ISubstitutedDeclaration declaration )
        => declaration switch
        {
            SubstitutedMethod substitutedMethod => this.GetMethod( substitutedMethod ),

            _ => throw new AssertionFailedException( $"Cannot get a declaration for a {declaration.GetType()}" )
        };

    public IType GetIType( IType type )
    {
        if ( ReferenceEquals( type.Compilation, this._compilationModel ) )
        {
            return type;
        }

        var typeImpl = (ITypeImpl) type;

        if ( typeImpl.TypeSymbol != null )
        {
            return this.GetIType( typeImpl.TypeSymbol );
        }
        else if ( typeImpl is BuiltNamedType builtNamedType )
        {
            return this.GetNamedType( builtNamedType.TypeBuilder, ReferenceResolutionOptions.Default ).AssertNotNull();
        }
        else
        {
            throw new AssertionFailedException( $"Constructions of introduced types are not supported." );
        }
    }

    [return: NotNullIfNotNull( "declaration" )]
    public T? GetDeclaration<T>( T? declaration, ReferenceResolutionOptions options = default )
        where T : class, IDeclaration
    {
        if ( declaration == null )
        {
            return null;
        }

        if ( ReferenceEquals( declaration.Compilation, this._compilationModel ) )
        {
            return declaration;
        }
        else if ( declaration is NamedType namedType )
        {
            return (T) this.GetNamedType( (INamedTypeSymbol) namedType.Symbol );
        }
        else
        {
            return declaration.ToValueTypedRef().GetTarget( this._compilationModel, options );
        }
    }

    public IConstructor GetConstructor( IConstructor attributeBuilderConstructor ) => this.GetDeclaration( attributeBuilderConstructor );

    public IParameter GetReturnParameter( IMethodSymbol methodSymbol ) => this.GetMethod( methodSymbol ).ReturnParameter;

    private Compilation Compilation => this._compilationModel.RoslynCompilation;

    private CompilationContext CompilationContext => this._compilationModel.CompilationContext;

    public Type GetReflectionType( ITypeSymbol typeSymbol ) => this._systemTypeResolver.GetCompileTimeType( typeSymbol, true ).AssertNotNull();

    public IAssembly GetAssembly( AssemblyIdentity assemblyIdentity )
    {
        if ( this.Compilation.Assembly.Identity.Equals( assemblyIdentity ) )
        {
            return this._compilationModel;
        }
        else
        {
            // TODO: performance
            var assemblySymbol = this.Compilation.SourceModule.ReferencedAssemblySymbols.Single( a => a.Identity.Equals( assemblyIdentity ) );

            return this.GetAssembly( assemblySymbol );
        }
    }

    internal DeserializedAttribute GetDeserializedAttribute( AttributeSerializationData serializationData )
        => this._deserializedAttributes.GetOrAdd(
            serializationData,
            static ( data, compilation ) => new DeserializedAttribute( data, compilation ),
            this._compilationModel );
}