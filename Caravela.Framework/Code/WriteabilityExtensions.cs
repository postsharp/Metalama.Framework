// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class WriteabilityExtensions
    {
        public static SetRelationship CompareWritability( this Writeability left, Writeability right )
            => new SetRelationship( left - right );
    }
}