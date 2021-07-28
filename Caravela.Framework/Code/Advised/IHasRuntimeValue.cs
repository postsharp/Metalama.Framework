// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Advised
{
    /// <summary>
    /// Something that has a value that can be read or assigned at run time - typically an <see cref="IAdvisedParameter"/>
    /// or an <see cref="IAdvisedFieldOrProperty"/>.
    /// </summary>
    [CompileTimeOnly]
    public interface IHasRuntimeValue
    {
        /// <summary>
        /// Gets or sets the value at run time.
        /// </summary>
        dynamic? Value { get; set; }
    }
}