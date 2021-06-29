// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Validation
{
    /// <summary>
    /// (Not implemented.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete( "Not implemented." )]
    [CompileTimeOnly]
    public interface IDeclarationReferenceValidator<T>
        where T : IDeclaration
    {
        void Initialize( IReadOnlyDictionary<string, string> properties );

        void ValidateReference( in ValidateReferenceContext<T> reference );
    }
}