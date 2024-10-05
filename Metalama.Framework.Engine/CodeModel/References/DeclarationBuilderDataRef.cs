using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface IDeclarationBuilderDataRef : IRefImpl
{
    DeclarationBuilderData BuilderData { get; }
}

internal sealed class DeclarationBuilderDataRef<T> : BaseRef<T>, IDeclarationBuilderDataRef
    where T : class, IDeclaration
{
    public DeclarationBuilderData BuilderData { get; }

    public DeclarationBuilderDataRef( DeclarationBuilderData builderData ) 
    {
        this.BuilderData = builderData;
    }

    public override IRef? ContainingDeclaration => this.BuilderData.ContainingDeclaration;

    public override string? Name => (this.BuilderData as NamedDeclarationBuilderData)?.Name;

    public override SerializableDeclarationId ToSerializableId() => throw new NotImplementedException();

    public override IRefImpl<TOut> As<TOut>() => (IRefImpl<TOut>) (object) this;

    public override bool Equals( IRef? other, RefComparison comparison )
        => other is DeclarationBuilderDataRef<T> otherRef && this.BuilderData == otherRef.BuilderData;

    public override int GetHashCode( RefComparison comparison ) => this.BuilderData.GetHashCode();

    public override DeclarationKind DeclarationKind => this.BuilderData.DeclarationKind;

    public override IDurableRef<T> ToDurable() => throw new NotImplementedException();

    protected override ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false ) => throw new NotSupportedException();

    protected override T? Resolve( CompilationModel compilation, bool throwIfMissing, IGenericContext? genericContext )
        => (T?) compilation.Factory.GetDeclaration( this.BuilderData, null, interfaceType: typeof(T) );

    public override ISymbol GetClosestContainingSymbol( CompilationContext compilationContext ) => throw new NotSupportedException();

    public override bool IsDurable => false;

    public override SerializableDeclarationId ToSerializableId( CompilationContext compilationContext ) => throw new NotImplementedException();

    public IRefImpl WithGenericContext( GenericContext genericContext )
     => genericContext.IsEmptyOrIdentity ? this : new BuiltDeclarationRef<T>( this.BuilderData, genericContext, genericContext.CompilationContext.AssertNotNull(  ) );
}