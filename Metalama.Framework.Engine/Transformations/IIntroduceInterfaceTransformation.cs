// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations
{
    internal interface IIntroduceInterfaceTransformation : IObservableTransformation, ITransformation
    {
        INamedType InterfaceType { get; }

        INamedType TargetType { get; }

        BaseTypeSyntax GetSyntax();
    }
}