// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Substituted;
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
using System.Linq;
using System.Threading.Tasks;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// Creates instances of <see cref="IDeclaration"/> for a given <see cref="CompilationModel"/>.
/// </summary>
[PublicAPI]
public sealed class DeclarationFactory : IDeclarationFactory, ISdkDeclarationFactory
{
    private readonly ConcurrentDictionary<Ref<ICompilationElement>, object> _defaultCache =
        new( RefEqualityComparer<ICompilationElement>.Default );

    // For types, we have a null-sensitive comparer to that 'object' and 'object?' are cached as two distinct items.
    private readonly ConcurrentDictionary<ITypeSymbol, object> _typeCache;

    private readonly ConcurrentDictionary<AttributeSerializationData, DeserializedAttribute> _deserializedAttributes = new();

    private readonly INamedType?[] _specialTypes = new INamedType?[(int) SpecialType.Count];
    private readonly INamedType?[] _internalSpecialTypes = new INamedType?[(int) InternalSpecialType.Count];

    private readonly CompilationModel _compilationModel;
    private readonly SystemTypeResolver _systemTypeResolver;

    internal DeclarationFactory( CompilationModel compilation )
    {
        this._compilationModel = compilation;
        this._typeCache = new ConcurrentDictionary<ITypeSymbol, object>( compilation.CompilationContext.SymbolComparerIncludingNullability );
        this._systemTypeResolver = compilation.Project.ServiceProvider.GetRequiredService<SystemTypeResolver>();
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

    internal INamespace GetNamespace( INamespaceSymbol namespaceSymbol )
        => (INamespace) this._defaultCache.GetOrAdd(
            namespaceSymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( l, c ) => new Namespace( (INamespaceSymbol) l.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );

    internal IAssembly GetAssembly( IAssemblySymbol assemblySymbol )
        => (IAssembly) this._defaultCache.GetOrAdd(
            assemblySymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( l, c )
                => !((IAssemblySymbol) l.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull()).Identity.Equals( c.RoslynCompilation.Assembly.Identity )
                    ? new ExternalAssembly( (IAssemblySymbol) l.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c )
                    : c,
            this._compilationModel );

    public IType GetIType( ITypeSymbol typeSymbol )
        => (IType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( l, c ) => CodeModelFactory.CreateIType( l, c ),
            this._compilationModel );

    private IArrayType GetArrayType( IArrayTypeSymbol typeSymbol )
        => (ArrayType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new ArrayType( (IArrayTypeSymbol) s, c ),
            this._compilationModel );

    private IDynamicType GetDynamicType( IDynamicTypeSymbol typeSymbol )
        => (DynamicType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new DynamicType( (IDynamicTypeSymbol) s, c ),
            this._compilationModel );

    private IPointerType GetPointerType( IPointerTypeSymbol typeSymbol )
        => (PointerType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new PointerType( (IPointerTypeSymbol) s, c ),
            this._compilationModel );

    public INamedType GetNamedType( INamedTypeSymbol typeSymbol, bool translateToCurrentCompilation = false )
    {
        // Roslyn considers the type in e.g. typeof(List<>) to be different from e.g. List<T>.
        // That distinction makes things more complicated for us (e.g. it's not representable using Type), so get rid of it.
        if ( typeSymbol.IsUnboundGenericType )
        {
            typeSymbol = typeSymbol.ConstructedFrom;
        }

        if ( translateToCurrentCompilation )
        {
            typeSymbol = typeSymbol.TranslateIfNecessary( this.CompilationContext );
        }
        else
        {
            typeSymbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );
        }

        return (INamedType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new NamedType( (INamedTypeSymbol) s, c ),
            this._compilationModel );
    }

    public ITypeParameter GetGenericParameter( ITypeParameterSymbol typeParameterSymbol )
        => (TypeParameter) this._typeCache.GetOrAdd(
            typeParameterSymbol,
            static ( tp, c ) => new TypeParameter( (ITypeParameterSymbol) tp, c ),
            this._compilationModel );

    public IMethod GetMethod( IMethodSymbol methodSymbol )
    {
        // Standardize on the partial definition part for partial methods.
        methodSymbol = methodSymbol.PartialDefinitionPart ?? methodSymbol;

        return (IMethod) this._defaultCache.GetOrAdd(
            methodSymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Method( (IMethodSymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );
    }

    public IProperty GetProperty( IPropertySymbol propertySymbol )
        => (IProperty) this._defaultCache.GetOrAdd(
            propertySymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Property( (IPropertySymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );

    public IIndexer GetIndexer( IPropertySymbol propertySymbol )
        => (IIndexer) this._defaultCache.GetOrAdd(
            propertySymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Indexer( (IPropertySymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );

    public IField GetField( IFieldSymbol fieldSymbol )
        => (IField) this._defaultCache.GetOrAdd(
            fieldSymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Field( (IFieldSymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );

    public IConstructor GetConstructor( IMethodSymbol methodSymbol, bool translateToCurrentCompilation = false )
    {
        if ( translateToCurrentCompilation )
        {
            methodSymbol = methodSymbol.TranslateIfNecessary( this.CompilationContext );
        }

        return (IConstructor) this._defaultCache.GetOrAdd(
            methodSymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Constructor( (IMethodSymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );
    }

    public IMethod GetFinalizer( IMethodSymbol finalizerSymbol )
        => (IMethod) this._defaultCache.GetOrAdd(
            finalizerSymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Method( (IMethodSymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );

    public IParameter GetParameter( IParameterSymbol parameterSymbol )
        => (IParameter) this._defaultCache.GetOrAdd(
            parameterSymbol.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Parameter( (IParameterSymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );

    public IEvent GetEvent( IEventSymbol @event )
        => (IEvent) this._defaultCache.GetOrAdd(
            @event.ToValueTypedRef( this.CompilationContext ).As<ICompilationElement>(),
            static ( ms, c ) => new Event( (IEventSymbol) ms.GetSymbol( c.RoslynCompilation ).AssertSymbolNotNull(), c ),
            this._compilationModel );

    public bool TryGetDeclaration( ISymbol symbol, [NotNullWhen( true )] out IDeclaration? declaration )
    {
        var compilationElement = this.GetCompilationElement( symbol );
        declaration = compilationElement as IDeclaration;

        return declaration != null;
    }

    internal IDeclaration? GetDeclarationOrNull( ISymbol symbol, DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
        => this.GetCompilationElement( symbol, kind ) as IDeclaration;

    public IDeclaration GetDeclaration( SymbolDictionaryKey key ) => this.GetDeclaration( key.GetSymbolId().Resolve( this.Compilation ).AssertSymbolNotNull() );

    public IDeclaration GetDeclaration( ISymbol symbol ) => this.GetDeclaration( symbol, DeclarationRefTargetKind.Default );

    internal IDeclaration GetDeclaration( ISymbol symbol, DeclarationRefTargetKind kind )
    {
        var compilationElement = this.GetCompilationElement( symbol, kind );

        if ( compilationElement is not IDeclaration declaration )
        {
            throw new ArgumentException( $"{symbol.Kind} is not a declaration", nameof(symbol) );
        }

        return declaration;
    }

    internal ICompilationElement? GetCompilationElement( ISymbol symbol, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
    {
        switch ( symbol.Kind )
        {
            case SymbolKind.Local:
            case SymbolKind.Label:
            case SymbolKind.ErrorType:
                return null;
        }

        symbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );

        switch ( symbol.Kind )
        {
            case SymbolKind.NamedType:
                {
                    var type = this.GetNamedType( (INamedTypeSymbol) symbol );

                    return targetKind switch
                    {
                        DeclarationRefTargetKind.StaticConstructor => type.StaticConstructor,
                        DeclarationRefTargetKind.Default => type,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

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
                        targetKind == DeclarationRefTargetKind.Return
                            ? this.GetReturnParameter( method )
                            : method.GetDeclarationKind() switch
                            {
                                DeclarationKind.Method => this.GetMethod( method ),
                                DeclarationKind.Constructor => this.GetConstructor( method ),
                                DeclarationKind.Finalizer => this.GetFinalizer( method ),
                                _ => throw new AssertionFailedException( $"Unexpected DeclarationRefTargetKind: {method.GetDeclarationKind()}." )
                            };
                }

            case SymbolKind.Property:
                var propertySymbol = (IPropertySymbol) symbol;

                var propertyOrIndexer = propertySymbol.IsIndexer
                    ? (IPropertyOrIndexer) this.GetIndexer( propertySymbol )
                    : this.GetProperty( propertySymbol );

                return targetKind switch
                {
                    // Implicit getter or setter.
                    DeclarationRefTargetKind.PropertyGet => propertyOrIndexer.GetMethod,
                    DeclarationRefTargetKind.PropertySet => propertyOrIndexer.SetMethod,

                    // The property itself.
                    DeclarationRefTargetKind.Default => propertyOrIndexer,
                    DeclarationRefTargetKind.Property => propertyOrIndexer,
                    _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                };

            case SymbolKind.Field:
                {
                    var field = this.GetField( (IFieldSymbol) symbol );

                    return targetKind switch
                    {
                        DeclarationRefTargetKind.Default => field,
                        DeclarationRefTargetKind.Field => field,
                        DeclarationRefTargetKind.PropertyGet => field.GetMethod,
                        DeclarationRefTargetKind.PropertySet => field.SetMethod,
                        DeclarationRefTargetKind.PropertyGetReturnParameter => field.GetMethod?.ReturnParameter,
                        DeclarationRefTargetKind.PropertySetParameter => field.SetMethod?.Parameters[0],
                        DeclarationRefTargetKind.PropertySetReturnParameter => field.SetMethod?.ReturnParameter,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.TypeParameter:
                return this.GetGenericParameter( (ITypeParameterSymbol) symbol );

            case SymbolKind.Parameter:
                return this.GetParameter( (IParameterSymbol) symbol );

            case SymbolKind.Event:
                {
                    var @event = this.GetEvent( (IEventSymbol) symbol );

                    return targetKind switch
                    {
                        DeclarationRefTargetKind.Default => @event,
                        DeclarationRefTargetKind.EventRaise => @event.RaiseMethod,
                        DeclarationRefTargetKind.EventRaiseParameter => throw new NotImplementedException(),
                        DeclarationRefTargetKind.EventRaiseReturnParameter => @event.RaiseMethod?.ReturnParameter,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.Assembly:
                return this.GetAssembly( (IAssemblySymbol) symbol );

            case SymbolKind.Namespace:
                return this.GetNamespace( (INamespaceSymbol) symbol );

            case SymbolKind.NetModule:
                return this._compilationModel;

            default:
                throw new AssertionFailedException( $"Don't know how to resolve a '{symbol.Kind}'." );
        }
    }

    IArrayType IDeclarationFactory.ConstructArrayType( IType elementType, int rank )
        => (IArrayType) this.GetIType(
            this.RoslynCompilation.CreateArrayTypeSymbol(
                ((ISdkType) elementType).TypeSymbol.AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ),
                rank ) );

    IPointerType IDeclarationFactory.ConstructPointerType( IType pointedType )
        => (IPointerType) this.GetIType(
            this.RoslynCompilation.CreatePointerTypeSymbol(
                ((ISdkType) pointedType).TypeSymbol.AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ) ) );

    public IType ConstructNullable( IType type, bool isNullable )
    {
        if ( type.IsNullable == isNullable )
        {
            return type;
        }

        var typeSymbol = ((ISdkType) type).TypeSymbol.AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes );
        ITypeSymbol newTypeSymbol;

        if ( type.IsReferenceType ?? true )
        {
            newTypeSymbol = typeSymbol
                .WithNullableAnnotation( isNullable ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated );
        }
        else
        {
            if ( isNullable )
            {
                newTypeSymbol = this._compilationModel.RoslynCompilation.GetSpecialType( Microsoft.CodeAnalysis.SpecialType.System_Nullable_T )
                    .Construct( typeSymbol );
            }
            else
            {
                return ((INamedType) type).TypeArguments[0];
            }
        }

        return this.GetIType( newTypeSymbol );
    }

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

        return declaration;
    }

    public T? Translate<T>( T compilationElement, ReferenceResolutionOptions options = ReferenceResolutionOptions.Default )
        where T : class, ICompilationElement
    {
        if ( ReferenceEquals( compilationElement.Compilation, this._compilationModel ) )
        {
            return compilationElement;
        }
        else
        {
            switch ( compilationElement )
            {
                case IDeclaration declaration:
                    return (T?) declaration.ToValueTypedRef().GetTargetOrNull( this._compilationModel, options );

                case IType type:
                    var translatedSymbol = this._compilationModel.CompilationContext.SymbolTranslator.Translate(
                        type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ) );

                    if ( translatedSymbol == null )
                    {
                        return null;
                    }

                    return (T) this._compilationModel.Factory.GetIType( translatedSymbol );

                default:
                    throw new AssertionFailedException( $"Cannot translate a '{compilationElement.GetType().Name}'." );
            }
        }
    }

    public IType GetTypeFromId( SerializableTypeId serializableTypeId, IReadOnlyDictionary<string, IType>? genericArguments )
        => this._compilationModel.SerializableTypeIdResolver.ResolveId( serializableTypeId, genericArguments );

    internal IAttribute GetAttribute( AttributeBuilder attributeBuilder, ReferenceResolutionOptions options )
        => (IAttribute) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( attributeBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltAttribute( (AttributeBuilder) l.Target!, c ),
            this._compilationModel );

    private static Exception CreateBuilderNotExists( IDeclarationBuilder builder )
        => new InvalidOperationException( $"The declaration '{builder}' does not exist in the current compilation." );

    private IParameter? GetParameter( BaseParameterBuilder parameterBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( parameterBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( parameterBuilder );
            }
            else
            {
                return null;
            }
        }

        return (IParameter) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( parameterBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltParameter( (BaseParameterBuilder) l.Target!, c ),
            this._compilationModel );
    }

    internal ITypeParameter GetGenericParameter( TypeParameterBuilder typeParameterBuilder, ReferenceResolutionOptions options )
        => (ITypeParameter) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( typeParameterBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltTypeParameter( (TypeParameterBuilder) l.Target!, c ),
            this._compilationModel );

    internal IMethod? GetMethod( MethodBuilder methodBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( methodBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( methodBuilder );
            }
            else
            {
                return null;
            }
        }

        return (IMethod) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( methodBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltMethod( c, (MethodBuilder) l.Target! ),
            this._compilationModel );
    }

    internal IMethod GetMethod( SubstitutedMethod substitutedMethod )
        => (IMethod) this._defaultCache.GetOrAdd(
            Ref.FromSubstitutedDeclaration( substitutedMethod ).As<ICompilationElement>(),
            static ( l, c ) =>
            {
                var original = (SubstitutedMethod) l.Target!;

                return new SubstitutedMethod(
                    original.SourceMethod,
                    Ref.FromSymbol( original.SubstitutedType, original.GetCompilationModel().CompilationContext )
                        .GetSymbol( c.RoslynCompilation )
                        .AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes )
                        .AssertCast<INamedTypeSymbol>() );
            },
            this._compilationModel );

    internal IMethod GetAccessor( AccessorBuilder methodBuilder, ReferenceResolutionOptions options )
        => (IMethod) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( methodBuilder ).As<ICompilationElement>(),
            static ( l, ctx ) =>
            {
                var builder = (AccessorBuilder) l.Target!;

                return ((IHasAccessors) ctx.me.GetDeclaration<IMember>( builder.ContainingMember, ctx.options )).GetAccessor( builder.MethodKind )!;
            },
            (me: this, options) );

    internal IConstructor? GetConstructor( ConstructorBuilder constructorBuilder, ReferenceResolutionOptions options, bool throwIfMissing = true )
    {
        if ( options.MustExist() && !this._compilationModel.Contains( constructorBuilder ) )
        {
            if ( throwIfMissing )
            {
                throw CreateBuilderNotExists( constructorBuilder );
            }
            else
            {
                return null;
            }
        }

        return (IConstructor) this._defaultCache.GetOrAdd(
            Ref.FromBuilder( constructorBuilder ).As<ICompilationElement>(),
            static ( l, c ) => new BuiltConstructor( c, (ConstructorBuilder) l.Target! ),
            this._compilationModel );
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