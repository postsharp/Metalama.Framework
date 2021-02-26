// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl
{
    internal abstract class IntroductionNameProvider
    {
        internal abstract string GetOverrideName( AspectLayerId advice, IMethod overriddenDeclaration );
    }
}