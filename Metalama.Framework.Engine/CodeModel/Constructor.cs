// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Impl.CodeModel
{
    internal class Constructor : MethodBase, IConstructor
    {
        public Constructor( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
            if ( symbol.MethodKind != MethodKind.Constructor && symbol.MethodKind != MethodKind.StaticConstructor )
            {
                throw new ArgumentOutOfRangeException( nameof(symbol), "The Constructor class must be used only with constructors." );
            }
        }

        public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        public ConstructorInfo ToConstructorInfo() => CompileTimeConstructorInfo.Create( this );

        public override System.Reflection.MethodBase ToMethodBase() => CompileTimeConstructorInfo.Create( this );
    }
}