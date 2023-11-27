// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class Constructor : MethodBase, IConstructorImpl
    {
        public Constructor( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
            if ( symbol.MethodKind != RoslynMethodKind.Constructor && symbol.MethodKind != RoslynMethodKind.StaticConstructor )
            {
                throw new ArgumentOutOfRangeException( nameof(symbol), "The Constructor class must be used only with constructors." );
            }
        }

        [Memo]
        public ConstructorInitializerKind InitializerKind
            => this.GetPrimaryDeclarationSyntax() switch
            {
                null => ConstructorInitializerKind.None,
                ConstructorDeclarationSyntax { Initializer: null } => ConstructorInitializerKind.None,
                ConstructorDeclarationSyntax { Initializer: { } initializer } when initializer.IsKind( SyntaxKind.ThisConstructorInitializer ) =>
                    ConstructorInitializerKind.This,
                ConstructorDeclarationSyntax { Initializer: { } initializer } when initializer.IsKind( SyntaxKind.BaseConstructorInitializer ) =>
                    ConstructorInitializerKind.Base,
#if ROSLYN_4_8_0_OR_GREATER
                ClassDeclarationSyntax { BaseList: null } =>
                    ConstructorInitializerKind.None,
                ClassDeclarationSyntax { BaseList: { } baseList } =>
                    baseList.Types.Any( bt => bt.IsKind( SyntaxKind.PrimaryConstructorBaseType ) )
                        ? ConstructorInitializerKind.Base
                        : ConstructorInitializerKind.None,
#endif
                _ => throw new AssertionFailedException( "Unexpected initializer for '{this}'." )
            };

        public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => Enumerable.Empty<IDeclaration>();

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        [Memo]
        public bool IsPrimary => this.MethodSymbol.IsPrimaryConstructor();

        public IMember? OverriddenMember => null;

        public IConstructor? GetBaseConstructor()
        {
            var declaration = this.GetPrimaryDeclarationSyntax();

            SyntaxNode? initializer = declaration switch
            {
                null => null,
                ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Initializer,
                TypeDeclarationSyntax typeDeclarationSyntax => typeDeclarationSyntax.BaseList?.Types.FirstOrDefault() as PrimaryConstructorBaseTypeSyntax,
                _ => throw new AssertionFailedException( $"Unexpected constructor syntax {declaration.GetType()}." )
            };

            if ( initializer == null )
            {
                // This is necessarily the default constructor of the base type, if any.
                return this.DeclaringType.BaseType?.Constructors.SingleOrDefault( c => c.Parameters.Count == 0 );
            }
            else
            {
                var semanticModel = this.GetCompilationModel().RoslynCompilation.GetCachedSemanticModel( declaration!.SyntaxTree );
                var symbol = (IMethodSymbol?) semanticModel.GetSymbolInfo( initializer ).Symbol;

                if ( symbol == null )
                {
                    return null;
                }
                else
                {
                    return this.GetCompilationModel().Factory.GetConstructor( symbol );
                }
            }
        }

        public ConstructorInfo ToConstructorInfo() => CompileTimeConstructorInfo.Create( this );

        [Memo]
        public IConstructor Definition
            => this.MethodSymbol == this.MethodSymbol.OriginalDefinition
                ? this
                : this.Compilation.Factory.GetConstructor( this.MethodSymbol.OriginalDefinition );

        protected override IMemberOrNamedType GetDefinition() => this.Definition;

        public override System.Reflection.MethodBase ToMethodBase() => CompileTimeConstructorInfo.Create( this );
    }
}