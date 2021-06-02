// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Linking;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Invoker
    {
        protected LinkerAnnotation LinkerAnnotation { get; }

        protected Invoker( IDeclaration declaration, LinkingOrder order )
        {
            this.LinkerAnnotation = new LinkerAnnotation( declaration.GetCompilationModel().AspectLayerId, order );
        }
    }
}