// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// A serializable object that stores the manifest of a <see cref="CompileTimeProject"/>. 
    /// </summary>
    internal class CompileTimeProjectManifest
    {
        /// <summary>
        /// Gets or sets the list of all aspect types (specified by fully qualified name) of the aspect library.
        /// </summary>
        public List<string>? AspectTypes { get; set; }

        /// <summary>
        /// Gets or sets the name of all project references (a fully-qualified assembly identity) of the compile-time project.
        /// </summary>
        public List<string>? References { get; set; }

        /// <summary>
        /// Gets or sets a unique hash of the source code and its dependencies.
        /// </summary>
        public ulong Hash { get; set; }
    }
}