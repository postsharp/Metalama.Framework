// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Services;

public sealed class CompilationContext : ICompilationServices, ITemplateReflectionContext
{
    internal CompilationContext( Compilation compilation )
    {
        this.Compilation = compilation;
    }

    [Memo]
    internal ResolvingCompileTimeTypeFactory CompileTimeTypeFactory => new( this.SerializableTypeIdResolver );

    [Memo]
    internal CompilationComparers Comparers => new( this.ReflectionMapper, this.Compilation );

    public Compilation Compilation { get; }

    CompilationModel ITemplateReflectionContext.GetCompilationModel( ICompilation sourceCompilation )
    {
        // When the current CompilationContext is used for reflecting the template code
        // (because the template is defined in source code, so it does not have its own ITemplateReflectionContext),
        // we use the source compilation.

        return (CompilationModel) sourceCompilation;
    }

    public bool IsCacheable => false;

    IReflectionMapper ICompilationServices.ReflectionMapper => this.ReflectionMapper;

    [Memo]
    internal ReflectionMapper ReflectionMapper => new( this.Compilation );

    [Memo]
    public SerializableTypeIdResolver SerializableTypeIdResolver => new( this.Compilation );

    [Memo]
    internal SyntaxGenerationContextFactory SyntaxGenerationContextFactory => new( this );

    [Memo]
    public SemanticModelProvider SemanticModelProvider => this.Compilation.GetSemanticModelProvider();

    [Memo]
    public SafeSymbolComparer SymbolComparer => new( this );

    [Memo]
    public IEqualityComparer<ISymbol> SymbolComparerIncludingNullability => new SafeSymbolComparer( this, SymbolEqualityComparer.IncludeNullability );

    [Memo]

    internal ImmutableDictionary<AssemblyIdentity, IAssemblySymbol> Assemblies
        => this.Compilation.SourceModule.ReferencedAssemblySymbols.Concat( this.Compilation.Assembly ).ToImmutableDictionary( x => x.Identity, x => x );

    [Memo]
    internal IEqualityComparer<MemberRef<INamedType>> NamedTypeRefComparer => new MemberRefEqualityComparer<INamedType>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<MemberRef<IConstructor>> ConstructorRefComparer => new MemberRefEqualityComparer<IConstructor>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<MemberRef<IEvent>> EventRefComparer => new MemberRefEqualityComparer<IEvent>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<MemberRef<IField>> FieldRefComparer => new MemberRefEqualityComparer<IField>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<MemberRef<IProperty>> PropertyRefComparer => new MemberRefEqualityComparer<IProperty>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<MemberRef<IIndexer>> IndexerRefComparer => new MemberRefEqualityComparer<IIndexer>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<MemberRef<IMethod>> MethodRefComparer => new MemberRefEqualityComparer<IMethod>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<IMember> MemberComparer => new MemberComparer<IMember>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<IEvent> EventComparer => new MemberComparer<IEvent>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<IField> FieldComparer => new MemberComparer<IField>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<IIndexer> IndexerComparer => new MemberComparer<IIndexer>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<IMethod> MethodComparer => new MemberComparer<IMethod>( this.SymbolComparer );

    [Memo]
    internal IEqualityComparer<IProperty> PropertyComparer => new MemberComparer<IProperty>( this.SymbolComparer );

    internal SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxNode node )
    {
        return SyntaxGenerationContext.Create( this, node );
    }

    internal SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxTree tree, int nodeSpanStart )
    {
        return SyntaxGenerationContext.Create( this, tree, nodeSpanStart );
    }

    [Memo]
    internal SyntaxGenerationContext DefaultSyntaxGenerationContext => this.GetSyntaxGenerationContext( false );

    internal SyntaxGenerationContext GetSyntaxGenerationContext( bool isPartial  )
    {
        return SyntaxGenerationContext.Create( this, isPartial );
    }

    [Memo]
    internal SymbolTranslator SymbolTranslator => new( this );
}