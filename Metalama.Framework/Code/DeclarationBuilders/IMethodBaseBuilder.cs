// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Code.DeclarationBuilders
{
    public interface IMethodBaseBuilder : IMethodBase, IMemberBuilder
    {
        /// <summary>
        /// Appends a parameter to the method.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="type">Parameter type.</param>
        /// <param name="refKind"><c>out</c>, <c>ref</c>...</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>A <see cref="IParameterBuilder"/> that allows you to further build the new parameter.</returns>
        IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default );

        /// <summary>
        /// Appends a parameter to the method.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="type">Parameter type.</param>
        /// <param name="refKind"><c>out</c>, <c>ref</c>...</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>A <see cref="IParameterBuilder"/> that allows you to further build the new parameter.</returns>
        IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null );
    }
}