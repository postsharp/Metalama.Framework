﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Transformations
{
    internal interface IReplaceMember : IObservableTransformation
    {
        MemberRef<IMemberOrNamedType>? ReplacedMember { get; }
    }
}