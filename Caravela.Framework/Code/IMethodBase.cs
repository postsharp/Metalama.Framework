// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Collections;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a method, constructor, or property.
    /// </summary>
    public interface IHasParameters : ICompilationElement, IDisplayable
    {
        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IParameterList Parameters { get; }
    }

    /// <summary>
    /// Represents a method or a constructor.
    /// </summary>
    public interface IMethodBase : IMember, IHasParameters
    {
        /// <summary>
        /// Gets the list of local functions declared by the current method.
        /// </summary>
        IMethodList LocalFunctions { get; }

        /// <summary>
        /// Gets the kind of method (such as <see cref="Code.MethodKind.Default"/> or <see cref="Code.MethodKind.PropertyGet"/>.
        /// </summary>
        MethodKind MethodKind { get; }

        /// <summary>
        /// Gets a <see cref="MethodBase"/> that represents the current method or constructor at run time.
        /// </summary>
        /// <returns>A <see cref="MethodBase"/> that can be used only in run-time code.</returns>
        [return: RunTimeOnly]
        MethodBase ToMethodBase();
    }
}