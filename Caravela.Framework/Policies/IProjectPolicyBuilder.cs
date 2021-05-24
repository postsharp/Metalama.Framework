// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Policies
{
    [InternalImplement]
    [Obsolete( "Not implemented." )]
    public interface IProjectPolicyBuilder
    {
        // The builder intentionally does not give write access to project properties. All configuration must use IProjectExtension.

        IProject Project { get; }

        // The builder intentionally does not give access to any ICompilation because project policies are compilation-independent.

        INamedTypeSet WithTypes( Func<ICompilation, IEnumerable<INamedType>> typeQuery );

        /// <summary>
        /// Adds a validator, which gets executed after all aspects have been added to the compilation.
        /// </summary>
        /// <param name="validator"></param>
        void AddValidator( Action<ValidateDeclarationContext<ICompilation>> validator );
    }
}