// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.References
{

    internal static class ReferenceResolutionOptionsExtensions
    {
        public static bool MustExist( this ReferenceResolutionOptions options ) => (options & ReferenceResolutionOptions.CanBeMissing) == 0;

        public static bool FollowRedirections( this ReferenceResolutionOptions options ) => (options & ReferenceResolutionOptions.DoNotFollowRedirections) == 0;
    }
}