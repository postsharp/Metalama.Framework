// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.Advising;

internal interface IIntroductionAdvice
{
    IDeclarationBuilder Builder { get; }
}