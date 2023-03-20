// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Invokers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a method, but not a constructor.
    /// </summary>
    public interface IMethod : IMethodBase, IGeneric, IMethodInvoker
    {
        /// <summary>
        /// Gets the kind of method (such as <see cref="Code.MethodKind.Default"/> or <see cref="Code.MethodKind.PropertyGet"/>.
        /// </summary>
        MethodKind MethodKind { get; }

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
        [Obsolete( "Use the methods of the IMethodInvoker interface that this object implements.", true )]
        IInvokerFactory<IMethodInvoker> Invokers { get; }

        /// <summary>
        /// Gets the base method that is overridden or hidden by the current method.
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
        [CompileTimeReturningRunTime]
        MethodInfo ToMethodInfo();

        /// <summary>
        /// Gets the parent property or event when the current <see cref="IMethod"/> represents a property or event accessor, otherwise <c>null</c>.
        /// </summary>
        IHasAccessors? DeclaringMember { get; }

        /// <summary>
        /// Gets a value indicating whether the method is <c>readonly</c>.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating the type of operator the methods represents.
        /// </summary>
        OperatorKind OperatorKind { get; }

        /// <summary>
        /// Gets the method definition with unassigned type parameters. When the current <see cref="IMethod"/> is neither a generic method instance
        /// nor a method of a generic type, returns the current <see cref="IMethod"/>.
        /// </summary>
        IMethod MethodDefinition { get; }

        /// <summary>
        /// Gets a value indicating whether the method has a non-managed implementation, i.e. has the <c>extern</c> modifier.
        /// </summary>
        bool IsExtern { get; }
    }
}