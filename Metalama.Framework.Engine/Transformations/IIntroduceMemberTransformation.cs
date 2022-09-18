// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents an introduced member (but not a type), observable or not.
    /// </summary>
    internal interface IIntroduceMemberTransformation : ITransformation
    {
        /// <summary>
        /// Gets the full syntax of introduced members including the body.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context );

        /// <summary>
        /// Gets the node after which the new members should be inserted.
        /// </summary>
        InsertPosition InsertPosition { get; }
    }
}