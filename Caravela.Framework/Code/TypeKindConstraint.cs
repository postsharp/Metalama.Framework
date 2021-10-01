// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    public enum TypeKindConstraint
    {
        None,
        Class,

        // TODO: Must be handled differently, as in Roslyn.
        NullableClass,
        Struct,
        Unmanaged,
        NotNull,
        Default
    }
}