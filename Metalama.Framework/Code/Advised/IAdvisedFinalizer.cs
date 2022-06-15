// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Advised
{
    public interface IAdvisedFinalizer : IFinalizer
    {
        /// <summary>
        /// Gets the list of method parameters.
        /// </summary>
        new IAdvisedParameterList Parameters { get; }
    }
}