// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Invokers;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a method, but not a constructor.
    /// </summary>
    public interface IMethod : IMethodBase, IGeneric
    {
        /// <summary>
        /// Gets an object representing the method return type and custom attributes, or  <c>null</c> for methods that don't have return types: constructors and finalizers.
        /// </summary>
        IParameter ReturnParameter { get; }

        /// <summary>
        /// Gets the method return type.
        /// </summary>
        IType ReturnType { get; }

        /// <summary>
        /// Gets an object that allows to invoke the current method.
        /// </summary>
        IInvokerFactory<IMethodInvoker> Invokers { get; }

        /// <summary>
        /// Gets the base method that is overridden by the current method.
        /// </summary>
        IMethod? OverriddenMethod { get; }

        /// <summary>
        /// Gets a list of interface methods that this method explicitly implements.
        /// </summary>
        IReadOnlyList<IMethod> ExplicitInterfaceImplementations { get; }

        /// <summary>
        /// Gets a <see cref="MethodInfo"/> that represents the current method at run time.
        /// </summary>
        /// <returns>A <see cref="MethodInfo"/> that can be used only in run-time code.</returns>
        MethodInfo ToMethodInfo();

        IMemberWithAccessors? DeclaringMember { get; }

        /// <summary>
        /// Gets a value indicating whether the method is <c>readonly</c>.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indiciting the type of operator the methods represents.
        /// </summary>
        OperatorKind OperatorKind { get; }
    }
}