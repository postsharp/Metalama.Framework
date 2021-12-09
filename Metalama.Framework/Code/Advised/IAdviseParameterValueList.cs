// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the list of method or property parameter values.
    /// </summary>
    [CompileTimeOnly]
    public interface IAdviseParameterValueList
    {
        /// <summary>
        /// Generates syntax that represents the current parameter list as an <c>object[]</c>.
        /// </summary>
        /// <returns></returns>
        dynamic ToArray();
    }
}