// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;

namespace Metalama.Framework.Engine.Transformations
{
    internal interface IIntroduceDeclarationTransformation : ITransformation
    {
        DeclarationBuilder DeclarationBuilder { get; }
    }
}