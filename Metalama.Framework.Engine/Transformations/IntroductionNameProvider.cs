// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Provides names for overriden declarations.
    /// </summary>
    internal abstract class IntroductionNameProvider
    {
        internal abstract string GetOverrideName( INamedType targetType, AspectLayerId aspectLayer, IMember overriddenMember );

        internal abstract string GetInitializerName( INamedType targetType, AspectLayerId aspectLayer, IMember initializedMember );
    }
}