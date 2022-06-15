// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class Finalizer : MethodBase, IFinalizerImpl
    {
        public Finalizer( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
            if ( symbol.MethodKind != MethodKind.Destructor )
            {
                throw new ArgumentOutOfRangeException( nameof(symbol), "The Finalizer class must be used only with finalizers." );
            }
        }

        public override DeclarationKind DeclarationKind => DeclarationKind.Finalizer;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        public override bool IsImplicit => this.GetSymbol().AssertNotNull().GetPrimarySyntaxReference() == null;

        public IMember? OverriddenMember => this.OverriddenFinalizer;

        public IFinalizer? OverriddenFinalizer
        {
            get
            {
                var overriddenFinalizer = this.MethodSymbol.OverriddenMethod;

                if ( overriddenFinalizer != null )
                {
                    return this.Compilation.Factory.GetFinalizer( overriddenFinalizer );
                }
                else
                {
                    return null;
                }
            }
        }

        public override System.Reflection.MethodBase ToMethodBase() => throw new NotSupportedException("Finalizers cannot be converted to reflection objects.");
    }
}