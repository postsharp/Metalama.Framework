﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Builders
{
    /// <summary>
    /// Allows to complete the construction of a field or property that has been created by an advice.
    /// </summary>
    public interface IFieldOrPropertyBuilder : IFieldOrProperty, IMemberBuilder
    {
        /// <summary>
        /// Gets or sets the type of the field or property.
        /// </summary>
        new IType Type { get; set; }
    }
}