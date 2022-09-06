// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the list of method or property parameter values.
    /// </summary>
    [CompileTime]
    public interface IAdviseParameterValueList
    {
        /// <summary>
        /// Generates syntax that represents the current parameter list as an <c>object[]</c>.
        /// </summary>
        /// <returns></returns>
        dynamic ToArray();
    }
}