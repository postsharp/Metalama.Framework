// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
    [CompileTimeOnly]
    public interface IMethodInvocation
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        [return: RunTimeOnly]
        dynamic Invoke( dynamic? instance, params dynamic[] args );
    }
}