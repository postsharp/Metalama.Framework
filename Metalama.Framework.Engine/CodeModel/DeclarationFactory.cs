// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
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
    // For types, we have a null-sensitive comparer to that 'object' and 'object?' are cached as two distinct items.
    private readonly ConcurrentDictionary<ITypeSymbol, IType> _typeCache;

    private readonly ConcurrentDictionary<AttributeSerializationData, DeserializedAttribute> _deserializedAttributes = new();

    private readonly INamedType?[] _specialTypes = new INamedType?[(int) SpecialType.Count];
    private readonly INamedType?[] _internalSpecialTypes = new INamedType?[(int) InternalSpecialType.Count];

    private readonly CompilationModel _compilationModel;
    private readonly CompileTimeTypeResolver _systemTypeResolver;

    internal DeclarationFactory( CompilationModel compilation )
    {
        this._compilationModel = compilation;
        this._symbolCache = new ConcurrentDictionary<ISymbol, IDeclaration>( compilation.CompilationContext.SymbolComparer );
        this._typeCache = new ConcurrentDictionary<ITypeSymbol, IType>( compilation.CompilationContext.SymbolComparerIncludingNullability );

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

        return declaration;
    }

    public T? Translate<T>(
        T? compilationElement,
        ReferenceResolutionOptions options = ReferenceResolutionOptions.Default,
        IGenericContext? genericContext = null )
        where T : class, ICompilationElement
        => (T?) ((ICompilationElementImpl?) compilationElement)?.Translate( this._compilationModel, options, genericContext );

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

    // We store a GenericMap and not a GenericContext because GenericMap implements IEquatable.
    private record struct BuilderCacheKey( IDeclarationBuilder Builder, GenericContext GenericContext );
}