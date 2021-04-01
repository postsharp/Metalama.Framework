// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Creates LinkerOverrideProceedImpl objects.
    /// </summary>
    internal class LinkerProceedImplementationFactory : ProceedImplementationFactory
    {
        public override IProceedImpl Get( AspectLayerId aspectLayerId, IMethod overriddenDeclaration )
        {
            return new LinkerOverrideProceedImpl( aspectLayerId, overriddenDeclaration );
        }
    }
}
