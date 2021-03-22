// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
    public interface IMethodInvocation
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic Invoke( dynamic? instance, params dynamic[] args );
    }
}