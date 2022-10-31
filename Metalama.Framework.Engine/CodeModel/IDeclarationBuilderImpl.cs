// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.CodeModel
{
    internal interface IDeclarationBuilderImpl : IDeclarationBuilder
    {
        Advice ParentAdvice { get; }
    }
}