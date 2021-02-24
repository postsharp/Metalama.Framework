using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents an introduced member (but not a type), observable or not.
    /// </summary>
    internal interface IMemberIntroduction : ISyntaxTreeTransformation
    {
        /// <summary>
        /// Gets the full syntax of introduced members including the body.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        /// <summary>
        /// Gets the node after which the new members should be inserted. If <see cref="InsertPositionNode"/> is set to a <see cref="TypeDeclarationSyntax "/>,
        /// the members will be appended to the end of the type. If it is set to a non-type member, the members will be inserted just after that member.
        /// </summary>
        MemberDeclarationSyntax InsertPositionNode { get; }
    }
}