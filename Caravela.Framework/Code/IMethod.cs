// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a method, but not a constructor.
    /// </summary>
    public interface IMethod : IMethodBase
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
        /// Gets the generic parameters of the current method.
        /// </summary>
        IGenericParameterList GenericParameters { get; }

        /// <summary>
        /// Gets the generic arguments of the current method.
        /// </summary>
        IReadOnlyList<IType> GenericArguments { get; }

        /// <summary>
        /// Gets a value indicating whether this method or any of its containers does not have generic arguments set.
        /// </summary>
        bool IsOpenGeneric { get; }

        /// <summary>
        /// Used for generic invocations. It returns an IMethod, not an IMethodInvocation, because
        /// it may be useful to evaluate the bound return and parameter types.
        /// </summary>
        IMethod WithGenericArguments( params IType[] genericArguments );

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


    }
}