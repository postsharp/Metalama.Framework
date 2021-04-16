// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents an introduction to the code model that should be observable by aspects running after the aspect that added the introduction. 
    /// </summary>
    internal interface IObservableTransformation
    {
        ICodeElement ContainingElement { get; }
    }

    internal interface IReplaceMemberTransformation : IObservableTransformation
    {
        MemberLink<IMember> ReplacedMember { get; }
    }
}