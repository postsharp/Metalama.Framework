// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Represents the metadata of an aspect class.
    /// </summary>
    public interface IAspectClassMetadata
    {
        /// <summary>
        /// Gets the fully qualified type of the aspect.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the short name of the aspect.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether the aspect class is an abstract class.
        /// </summary>
        bool IsAbstract { get; }
    }
}