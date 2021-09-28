// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Interface )]
    public sealed class OriginalIdAttribute : Attribute
    {
        public string Id { get; }

        public OriginalIdAttribute( string id )
        {
            this.Id = id;
        }
    }
}