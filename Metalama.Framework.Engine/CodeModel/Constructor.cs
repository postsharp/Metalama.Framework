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
    internal class Constructor : MethodBase, IConstructorImpl
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
            => (ConstructorDeclarationSyntax?) this.GetPrimaryDeclarationSyntax() switch
            {
                null => ConstructorInitializerKind.None,
                { Initializer: null } => ConstructorInitializerKind.None,
                { Initializer: { } initializer } when initializer.IsKind( SyntaxKind.ThisConstructorInitializer ) =>
                    ConstructorInitializerKind.This,
                { Initializer: { } initializer } when initializer.IsKind( SyntaxKind.BaseConstructorInitializer ) =>
                    ConstructorInitializerKind.Base,
                _ => throw new AssertionFailedException( "Unexpected initializer for '{this}'." )
            };

        public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        public IMember? OverriddenMember => null;

        public IConstructor? GetBaseConstructor()
        {
            var declaration = (ConstructorDeclarationSyntax?) this.GetPrimaryDeclarationSyntax();

            if ( declaration == null || declaration.Initializer == null )
            {
                // This is necessarily the default constructor of the base type, if any.
                return this.DeclaringType.BaseType?.Constructors.SingleOrDefault( c => c.Parameters.Count == 0 );
            }
            else
            {
                var semanticModel = this.GetCompilationModel().RoslynCompilation.GetCachedSemanticModel( declaration.SyntaxTree );
                var symbol = (IMethodSymbol?) semanticModel.GetSymbolInfo( declaration.Initializer ).Symbol;

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

        public override System.Reflection.MethodBase ToMethodBase() => CompileTimeConstructorInfo.Create( this );
    }
}