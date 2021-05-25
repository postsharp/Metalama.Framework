// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Validation
{
    [Obsolete( "Not implemented." )]
    public interface IDeclarationReferenceValidator<T>
        where T : IDeclaration
    {
        void Initialize( IReadOnlyDictionary<string, string> properties );

        void ValidateReference( in ValidateReferenceContext<T> reference );
    }
}