// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code.Builders
{
    /// <summary>
    /// Allows to complete the construction ofa property that has been created by an advice.
    /// </summary>
    public interface IPropertyBuilder : IFieldOrPropertyBuilder, IProperty
    {
        /// <summary>
        /// Gets or sets the <see cref="Caravela.Framework.Code.RefKind"/> of the property
        /// (i.e. <see cref="Code.RefKind.Ref"/>, <see cref="Code.RefKind.Out"/>, ...).
        /// </summary>
        new RefKind RefKind { get; set; }

        /// <summary>
        /// Adds a parameter to the current indexer and specifies its type using an <see cref="IType"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="refKind"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default );

        /// <summary>
        /// Adds a parameter to the current indexer and specifies its type using a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="refKind"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null );

        /// <summary>
        /// Gets the <see cref="IMethodBuilder"/> for the getter.
        /// </summary>
        new IMethodBuilder? Getter { get; }

        /// <summary>
        /// Gets the <see cref="IMethodBuilder"/> for the setter.
        /// </summary>
        new IMethodBuilder? Setter { get; }
    }
}