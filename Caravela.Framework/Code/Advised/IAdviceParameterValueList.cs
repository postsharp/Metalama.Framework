// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Advised
{
    /// <summary>
    /// Represents the list of method or property parameter values.
    /// </summary>
    [CompileTimeOnly]
    public interface IAdviceParameterValueList
    {
        /// <summary>
        /// Generates syntax that represents the current parameter list as an <c>object[]</c>.
        /// </summary>
        /// <returns></returns>
        [return: RunTimeOnly]
        dynamic ToArray();

        /// <summary>
        /// Generates syntax that represents the current parameter list as a tuple, like <c>(a, b)</c>.
        /// </summary>
        /// <returns></returns>
        [return: RunTimeOnly]
        dynamic ToValueTuple();
    }
}