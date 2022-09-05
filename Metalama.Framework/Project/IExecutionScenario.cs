// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Project
{
    /// <summary>
    /// Exposes the properties of the scenarios in which an aspect, template, or fabric is being executed. The interface is accessible
    /// from the <see cref="MetalamaExecutionContext"/> class.
    /// </summary>
    [CompileTime]
    [InternalImplement]
    public interface IExecutionScenario
    {
        /// <summary>
        /// Gets the name of the scenario. Callers should not rely on this scenario. Instead, it should rely on the other properties
        /// of this interface.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether Metalama is currently executing at design time. 
        /// </summary>
        bool IsDesignTime { get; }

        /// <summary>
        /// Gets a value indicating whether the current execution context is interested by non-observable transformations, i.e. transformations
        /// that affect the implementation of an existing declaration, but does not add, remove or modify characteristics that are observable
        /// outside of the declaration. It is generally useless to add non-observable transformations in a context that does not support them
        /// (they will be ignored anyway).
        /// </summary>
        bool CapturesNonObservableTransformations { get; }

        /// <summary>
        /// Gets a value indicating whether the current execution context is interested by the implementation of code fixes. The only
        /// situation when Metalama is interested by the code fix implementation is when the user actually selects a code fix, either
        /// for preview or for execution. In other scenarios, the implementation is dropped. 
        /// </summary>
        bool CapturesCodeFixImplementations { get; }

        /// <summary>
        /// Gets a value indicating whether the current execution context is interested by the titles of code fixes. This does <i>not</i> imply
        /// <see cref="CapturesCodeFixImplementations"/>.
        /// </summary>
        bool CapturesCodeFixTitles { get; }
    }
}