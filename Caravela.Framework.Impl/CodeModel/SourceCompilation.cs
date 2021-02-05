﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class SourceCompilation : BaseCompilation
    {
        internal CSharpCompilation RoslynCompilation { get; }

        internal SymbolMap SymbolMap { get; }

        public SourceCompilation( CSharpCompilation roslynCompilation )
        {
            this.RoslynCompilation = roslynCompilation;

            this.SymbolMap = new( this );
        }

        [Memo]
        public override IReactiveCollection<INamedType> DeclaredTypes =>
            this.RoslynCompilation.Assembly.GetTypes().Select( this.SymbolMap.GetNamedType ).ToImmutableReactive();

        [Memo]
        public override IReactiveCollection<INamedType> DeclaredAndReferencedTypes =>
            this.RoslynCompilation.GetTypes().Select( this.SymbolMap.GetNamedType ).ToImmutableReactive();

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes =>
            this.RoslynCompilation.Assembly.GetAttributes().Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                .Select( a => new Attribute( a, this.SymbolMap ) )
                .ToImmutableReactive();

        public override INamedType? GetTypeByReflectionName( string reflectionName )
        {
            var symbol = this.RoslynCompilation.GetTypeByMetadataName( reflectionName );

            return symbol == null ? null : this.SymbolMap.GetNamedType( symbol );
        }

        internal override CSharpCompilation GetPrimeCompilation() => this.RoslynCompilation;

        internal override IReactiveCollection<AdviceInstance> CollectAdvices() => ImmutableArray.Create<AdviceInstance>().ToReactive();

        internal override CSharpCompilation GetRoslynCompilation() => this.RoslynCompilation;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";
    }

    internal static class Factory
    {
        internal static IType CreateIType( ITypeSymbol typeSymbol, SourceCompilation compilation ) =>
            typeSymbol switch
            {
                INamedTypeSymbol namedType => new NamedType( namedType, compilation ),
                IArrayTypeSymbol arrayType => new ArrayType( arrayType, compilation ),
                IPointerTypeSymbol pointerType => new PointerType( pointerType, compilation ),
                ITypeParameterSymbol typeParameter => new GenericParameter( typeParameter, compilation ),
                IDynamicTypeSymbol dynamicType => new DynamicType( dynamicType, compilation ),
                _ => throw new NotImplementedException()
            };
    }

    // for testing
    internal static class CompilationFactory
    {
        public static ICompilation CreateCompilation( CSharpCompilation roslynCompilation ) => new SourceCompilation( roslynCompilation );
    }
}
