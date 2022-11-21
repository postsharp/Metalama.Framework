// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CodeFixes;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeFixes
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

        /// <summary>
        /// Gets some indication about the creator of the code fix.
        /// </summary>
        public string Creator { get; }

        /// <summary>
        /// Gets a value indicating whether this aspect is licensed.
        /// </summary>
        public bool IsLicensed { get; }

        internal CodeFixInstance( string diagnosticId, Location location, CodeFix codeFix, string creator, bool isLicensed )
        {
            this.DiagnosticId = diagnosticId;
            this.Location = location;
            this.CodeFix = codeFix;
            this.Creator = creator;
            this.IsLicensed = isLicensed;
        }
    }
}