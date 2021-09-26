// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Fabrics
{
    [InternalImplement]
    [CompileTimeOnly]
    public interface INamespaceFabricBuilder
    {
        IProject Project { get; }

        // The builder intentionally does not give access to the INamespace because they must be compilation-neutral.

        INamedTypeSelection WithTypes( Func<INamespace, IEnumerable<INamedType>> typeQuery );

        [Obsolete("Not implemented.")]
        void AddValidator( Action<ValidateDeclarationContext<INamespace>> validator );

        // TODO: Adding reference validators to namespaces is problematic
    }
}