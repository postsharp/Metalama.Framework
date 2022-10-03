// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations
{
    internal interface IIntroduceInterfaceTransformation : IObservableTransformation
    {
        INamedType InterfaceType { get; }

        INamedType TargetType { get; }

        BaseTypeSyntax GetSyntax();
    }
}