// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    public static class SymbolExtensions
    {
        public static ISymbol? GetSymbol( this IDeclaration declaration ) => ((ISdkDeclaration) declaration).Symbol;

        public static ISymbol? GetSymbol( this IRef<ICompilationElement> declaration, Compilation compilation, bool ignoreAssemblyKey = false )
            => ((ISdkRef<ICompilationElement>) declaration).GetSymbol( compilation, ignoreAssemblyKey );

        private static T? GetSymbol<T>( this IDeclaration declaration )
            where T : ISymbol
            => (T?) ((ISdkDeclaration) declaration).Symbol;

        public static ITypeSymbol GetSymbol( this IType type )
            => ((ISdkType) type).TypeSymbol ?? throw new InvalidOperationException(
                "Assertion failed: until type introductions are supported, all types are assumed to have a Roslyn symbol." );

        public static INamedTypeSymbol GetSymbol( this INamedType namedType )
            => namedType.GetSymbol<INamedTypeSymbol>() ?? throw new InvalidOperationException(
                "Assertion failed: until type introductions are supported, all types are assumed to have a Roslyn symbol." );

        public static IMethodSymbol? GetSymbol( this IMethodBase method ) => method.GetSymbol<IMethodSymbol>();

        public static IPropertySymbol? GetSymbol( this IProperty property ) => property.GetSymbol<IPropertySymbol>();

        public static IEventSymbol? GetSymbol( this IEvent @event ) => @event.GetSymbol<IEventSymbol>();

        public static IFieldSymbol? GetSymbol( this IField field ) => field.GetSymbol<IFieldSymbol>();

        public static IParameterSymbol? GetSymbol( this IParameter parameter ) => parameter.GetSymbol<IParameterSymbol>();

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