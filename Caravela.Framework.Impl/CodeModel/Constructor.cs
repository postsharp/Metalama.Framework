// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Constructor : MethodBase, IConstructor
    {
        public Constructor( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
        }

        public override CodeElementKind ElementKind => CodeElementKind.Constructor;

        public override bool IsReadOnly => false;

        public override bool IsAsync => false;
    }
}