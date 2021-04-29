// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a method or a constructor.
    /// </summary>
    public interface IMethodBase : IMember
    {
        /// <summary>
        /// Gets the list of local functions declared by the current method.
        /// </summary>
        IMethodList LocalFunctions { get; }

        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IParameterList Parameters { get; }

        /// <summary>
        /// Gets the kind of method (such as <see cref="Code.MethodKind.Default"/> or <see cref="Code.MethodKind.PropertyGet"/>.
        /// </summary>
        MethodKind MethodKind { get; }

        [return: RunTimeOnly]
         MethodBase ToMethodBase();
    }
}