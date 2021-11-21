// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// Exposes a <see cref="DiagnosticLocation"/> property that determines the location of a user-code diagnostic.
    /// This interface is implemented by <see cref="IDeclaration"/>.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    [CompileTimeOnly]
    [InternalImplement]
    public interface IDiagnosticLocation { }
}