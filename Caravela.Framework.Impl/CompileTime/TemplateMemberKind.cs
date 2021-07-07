// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Kinds of template members.
    /// </summary>
    internal enum TemplateMemberKind
    {
        /// <summary>
        /// Not a template.
        /// </summary>
        None,

        /// <summary>
        /// Template for programmatic advice.
        /// </summary>
        Template,

        /// <summary>
        /// Introduction advice.
        /// </summary>
        Introduction,

        /// <summary>
        /// Interface member.
        /// </summary>
        InterfaceMember
    }
}