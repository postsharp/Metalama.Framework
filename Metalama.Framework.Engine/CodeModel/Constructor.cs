// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class Constructor : MethodBase, IConstructorImpl
    {
        public Constructor( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
            if ( symbol.MethodKind != MethodKind.Constructor && symbol.MethodKind != MethodKind.StaticConstructor )
            {
                throw new ArgumentOutOfRangeException( nameof(symbol), "The Constructor class must be used only with constructors." );
            }
        }

        [Memo]
        public ConstructorInitializerKind InitializerKind
            => (ConstructorDeclarationSyntax?) this.GetPrimaryDeclaration() switch
            {
                null => ConstructorInitializerKind.Undetermined,
                { Initializer: null } => ConstructorInitializerKind.Undetermined,
                { Initializer: { } initializer } when initializer.Kind() == SyntaxKind.ThisConstructorInitializer =>
                    ConstructorInitializerKind.This,
                { Initializer: { } initializer } when initializer.Kind() == SyntaxKind.BaseConstructorInitializer =>
                    ConstructorInitializerKind.Base,
                _ => throw new AssertionFailedException()
            };

        public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        public override bool IsImplicit => this.GetSymbol().AssertNotNull().GetPrimarySyntaxReference() == null;

        public IMember? OverriddenMember => null;

        public ConstructorInfo ToConstructorInfo() => CompileTimeConstructorInfo.Create( this );

        public override System.Reflection.MethodBase ToMethodBase() => CompileTimeConstructorInfo.Create( this );
    }
}