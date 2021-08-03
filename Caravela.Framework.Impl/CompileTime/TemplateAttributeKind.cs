// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Kinds of template members as specified by their attribute.
    /// </summary>
    internal enum TemplateAttributeKind
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
        InterfaceMember,

        /// <summary>
        /// A template that does not have any implementation, either because it is abstract,
        /// either because of <see cref="AbstractAttribute"/>.
        /// </summary>
        Abstract
    }
}