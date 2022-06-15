// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations
{
    internal abstract class AspectReferenceSyntaxProvider
    {
        // TODO: other kinds of aspect references.

        public abstract ExpressionSyntax GetFinalizerReference( AspectLayerId aspectLayer, IFinalizer overriddenFinalizer, OurSyntaxGenerator syntaxGenerator );
    }
}