// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.RunTime;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// A base interface for <see cref="IField"/> and <see cref="IProperty"/>.
    /// </summary>
    public interface IFieldOrProperty : IFieldOrPropertyOrIndexer
    {
        /// <summary>
        /// Gets a value indicating whether the declaration is an auto-property or a field, or <c>null</c> if the
        /// implementation of the property cannot be determined, for instance for properties in a referenced assembly.
        /// </summary>
        /// <remarks>
        /// When an automatic property has been overridden, this property will still return <c>true</c>, even if the actual
        /// property implementation no longer corresponds to the one of an automatic property.
        /// </remarks>
        bool? IsAutoPropertyOrField { get; }

        /// <summary>
        /// Gets an object that allows to get or set the value of the current field or property.
        /// </summary>
        IInvokerFactory<IFieldOrPropertyInvoker> Invokers { get; }

        /// <summary>
        /// Gets a <see cref="FieldOrPropertyOrIndexerInfo"/> that represents the current field or property at run time.
        /// </summary>
        /// <returns>A <see cref="FieldOrPropertyOrIndexerInfo"/> that can be used only in run-time code.</returns>
        [CompileTimeReturningRunTime]
        FieldOrPropertyOrIndexerInfo ToFieldOrPropertyOrIndexerInfo();
    }
}