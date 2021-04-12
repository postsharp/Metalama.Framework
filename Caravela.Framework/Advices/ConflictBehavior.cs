// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Advices
{
    // TODO: Document.
    public enum ConflictBehavior
    {
        Default = Fail,

        // Fails with a compile error.
        Fail = 0,

        // Ignores the advice if the member already exists.
        Ignore = 1,

        // Tries to override, or fails if this is not possible 
        // (i.e. sealed class in parent class)
        Override = 2,

        // Tries to define the member as `new`, or fails if this is not possible
        // (i.e. sealed class in parent class)
        New = 3,

        // Only for types. Continues introducing the members. Members are processed with the Ignore behavior.
        Merge = 4
    }
}
