// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations
{
    internal interface IInjectInterfaceTransformation : ITransformation
    {
        BaseTypeSyntax GetSyntax();
    }
}