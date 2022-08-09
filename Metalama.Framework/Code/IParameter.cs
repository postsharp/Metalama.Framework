// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a parameter of a method or property.
    /// </summary>
    public interface IParameter : INamedDeclaration, IHasType
    {
        /// <summary>
        /// Gets the <c>in</c>, <c>out</c>, <c>ref</c> parameter type modifier.
        /// </summary>
        RefKind RefKind { get; }

        /// <summary>
        /// Gets the parameter position, or <c>-1</c> for <see cref="IMethod.ReturnParameter"/>.
        /// </summary>
        int Index { get; }

        /// <remarks>
        /// Gets the default value of the parameter, or  <c>default</c> if the parameter type is a struct and the default
        /// value of the parameter is the default value of the struct type.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The parameter has no default value.</exception>
        TypedConstant? DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter has the <c>params</c> modifier.
        /// </summary>
        bool IsParams { get; }

        /// <summary>
        /// Gets the parent <see cref="IMethod"/>, <see cref="IConstructor"/> or <see cref="IProperty"/>.
        /// </summary>
        IHasParameters DeclaringMember { get; }

        /// <summary>
        /// Gets a <see cref="ParameterInfo"/> that represents the current parameter at run time.
        /// </summary>
        /// <returns>A <see cref="ParameterInfo"/> that can be used only in run-time code.</returns>
        ParameterInfo ToParameterInfo();

        bool IsReturnParameter { get; }
    }
}