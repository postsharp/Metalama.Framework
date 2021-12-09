// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Kinds of template members as specified by their attribute.
    /// </summary>
    internal enum TemplateAttributeType
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
        /// Declarative introduction advice.
        /// </summary>
        Introduction,

        /// <summary>
        /// Interface member.
        /// </summary>
        InterfaceMember
    }
}