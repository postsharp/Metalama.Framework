// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Invokers;
using Metalama.Framework.RunTime;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// A base interface for <see cref="IField"/> and <see cref="IProperty"/>.
    /// </summary>
    public interface IFieldOrProperty : IPropertyLike
    {
        /// <summary>
        /// Gets a value indicating whether the declaration is an auto-property or a field.
        /// </summary>
        bool IsAutoPropertyOrField { get; }

        /// <summary>
        /// Gets an object that allows to get or set the value of the current field or property.
        /// </summary>
        IInvokerFactory<IFieldOrPropertyInvoker> Invokers { get; }

        /// <summary>
        /// Gets a <see cref="FieldOrPropertyInfo"/> that represents the current field or property at run time.
        /// </summary>
        /// <returns>A <see cref="FieldOrPropertyInfo"/> that can be used only in run-time code.</returns>
        FieldOrPropertyInfo ToFieldOrPropertyInfo();
    }
}