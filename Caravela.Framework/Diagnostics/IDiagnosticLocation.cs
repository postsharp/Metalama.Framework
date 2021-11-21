// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// An base interface for objects to which a diagnostic can be reported.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    [CompileTimeOnly]
    [InternalImplement]
    public interface IDiagnosticLocation { }
}