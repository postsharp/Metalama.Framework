// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a <see cref="Metalama.Framework.CodeFixes.CodeFix"/> that has been suggested for a diagnostic.
    /// </summary>
    public sealed class CodeFixInstance
    {
        /// <summary>
        /// Gets the suggested <see cref="Metalama.Framework.CodeFixes.CodeFix"/>.
        /// </summary>
        public CodeFix CodeFix { get; }

        /// <summary>
        /// Gets some indication about the creator of the code fix.
        /// </summary>
        internal string Creator { get; }

        /// <summary>
        /// Gets a value indicating whether this aspect is licensed.
        /// </summary>
        internal bool IsLicensed { get; }

        internal CodeFixInstance( CodeFix codeFix, string creator, bool isLicensed )
        {
            this.CodeFix = codeFix;
            this.Creator = creator;
            this.IsLicensed = isLicensed;
        }
    }
}