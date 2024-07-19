// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Exposes the <see cref="ISymbol" /> from <see cref="IDeclaration"/>.
    /// </summary>
    [PublicAPI]
    public static class SymbolExtensions
    {
        public static ISymbol? GetSymbol( this IDeclaration declaration ) => ((ISdkDeclaration) declaration).Symbol;

        public static ISymbol? GetSymbol( this IRef<ICompilationElement> declaration, Compilation compilation, bool ignoreAssemblyKey = false )
            => ((ISdkRef<ICompilationElement>) declaration).GetSymbol( compilation, ignoreAssemblyKey );

        private static T? GetSymbol<T>( this IDeclaration declaration )
            where T : ISymbol
            => (T?) ((ISdkDeclaration) declaration).Symbol;

        public static ITypeSymbol? GetSymbol( this IType type ) => ((ISdkType) type).TypeSymbol;

        public static INamedTypeSymbol? GetSymbol( this INamedType namedType ) => namedType.GetSymbol<INamedTypeSymbol>();

        public static ITypeParameterSymbol? GetSymbol( this ITypeParameter typeParameter ) => typeParameter.GetSymbol<ITypeParameterSymbol>();

        public static IMethodSymbol? GetSymbol( this IMethodBase method ) => method.GetSymbol<IMethodSymbol>();

        public static IPropertySymbol? GetSymbol( this IProperty property ) => property.GetSymbol<IPropertySymbol>();

        public static IEventSymbol? GetSymbol( this IEvent @event ) => @event.GetSymbol<IEventSymbol>();

        public static IFieldSymbol? GetSymbol( this IField field ) => field.GetSymbol<IFieldSymbol>();

        public static IParameterSymbol? GetSymbol( this IParameter parameter ) => parameter.GetSymbol<IParameterSymbol>();

        public static IAssemblySymbol GetSymbol( this IAssembly assembly ) => assembly.GetSymbol<IAssemblySymbol>();

        public static ISymbol? GetOverriddenMember( this ISymbol? symbol )
            => symbol switch
            {
                IMethodSymbol method => method.OverriddenMethod,
                IPropertySymbol property => property.OverriddenProperty,
                IEventSymbol @event => @event.OverriddenEvent,
                _ => null
            };

        public static ITypeSymbol? GetExpressionType( this ISymbol symbol )
        {
            var type = ExpressionTypeVisitor.Instance.Visit( symbol );

            if ( type is { SpecialType: SpecialType.System_Void } )
            {
                return null;
            }
            else
            {
                return type;
            }
        }

        public static Compilation GetRoslynCompilation( this ICompilation compilation ) => ((ISdkCompilation) compilation).RoslynCompilation;

        public static SemanticModel GetSemanticModel( this ICompilation compilation, SyntaxTree syntaxTree )
            => ((ISdkCompilation) compilation).GetCachedSemanticModel( syntaxTree );

        public static bool TryGetDeclaration( this ICompilation compilation, ISymbol symbol, out IDeclaration? declaration )
            => ((ISdkCompilation) compilation).Factory.TryGetDeclaration( symbol, out declaration );

        public static IDeclaration GetDeclaration( this ICompilation compilation, ISymbol symbol )
        {
            if ( !compilation.TryGetDeclaration( symbol, out var declaration ) )
            {
                throw new ArgumentOutOfRangeException( nameof(symbol), $"The symbol '{symbol}' cannot be mapped to the compilation '{compilation}'." );
            }

            return declaration;
        }

        private sealed class ExpressionTypeVisitor : SymbolVisitor<ITypeSymbol>
        {
            public static ExpressionTypeVisitor Instance { get; } = new();

            public override ITypeSymbol VisitEvent( IEventSymbol symbol ) => symbol.Type;

            public override ITypeSymbol VisitField( IFieldSymbol symbol ) => symbol.Type;

            public override ITypeSymbol VisitLocal( ILocalSymbol symbol ) => symbol.Type;

            public override ITypeSymbol VisitMethod( IMethodSymbol symbol ) => symbol.ReturnType;

            public override ITypeSymbol VisitParameter( IParameterSymbol symbol ) => symbol.Type;

            public override ITypeSymbol VisitProperty( IPropertySymbol symbol ) => symbol.Type;

            public override ITypeSymbol? DefaultVisit( ISymbol symbol ) => null;
        }
    }
}