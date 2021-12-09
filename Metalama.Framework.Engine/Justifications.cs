// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine
{
    internal static class Justifications
    {
        public const string SerializersNotImplementedForIntroductions = "Serializers are not implemented for introductions.";
        public const string TemplateMembersHaveSymbol = "Template members are known to have symbols.";
        public const string TypesAlwaysHaveSymbol = "Types are known to have symbols.";
        public const string ImplementingIntroducedInterfacesNotSupported = "Implementing introduced interfaces is not supported.";
        public const string CoverageMissing = "Coverage of this code is missing and it is unclear how to reproduce it. Should be treated as a bug.";
        public const string ObsoleteBranch = "This code should not execute under current implementation. Should be treated as a bug.";
    }
}