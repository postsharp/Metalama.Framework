// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.CodeFixes;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a <see cref="Metalama.Framework.CodeFixes.CodeFix"/> that has been suggested for a diagnostic.
    /// </summary>
    public class CodeFixInstance
    {
        /// <summary>
        /// Gets the suggested <see cref="Metalama.Framework.CodeFixes.CodeFix"/>.
        /// </summary>
        public CodeFix CodeFix { get; }

        /// <summary>
        /// Gets the id of the diagnostic for which the code fix was suggested.
        /// </summary>
        public string DiagnosticId { get; }

        /// <summary>
        /// Gets the location of the diagnostic for which the code fix was suggested.
        /// </summary>
        public Location Location { get; }

        internal CodeFixInstance( string diagnosticId, Location location, CodeFix codeFix )
        {
            this.DiagnosticId = diagnosticId;
            this.Location = location;
            this.CodeFix = codeFix;
        }
    }
}