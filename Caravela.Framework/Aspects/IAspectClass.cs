﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents the metadata of an aspect class.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAspectClass
    {
        /// <summary>
        /// Gets the fully qualified type of the aspect.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the short name of the aspect.
        /// </summary>
        string DisplayName { get; }

        string? Description { get; }

        /// <summary>
        /// Gets a value indicating whether the aspect class is an abstract class.
        /// </summary>
        bool IsAbstract { get; }
    }
}