// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Provides extension methods for <see cref="IParameter"/>.
    /// </summary>
    public static class ParameterExtensions
    {
        public static bool IsByRef( this IParameter parameter ) => parameter.RefKind != RefKind.None;

        public static bool IsRef( this IParameter parameter ) => parameter.RefKind == RefKind.Ref;

        public static bool IsOut( this IParameter parameter ) => parameter.RefKind == RefKind.Out;
    }
}