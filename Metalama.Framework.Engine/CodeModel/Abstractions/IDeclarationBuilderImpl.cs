// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;

namespace Metalama.Framework.Engine.CodeModel.Abstractions
{
    internal interface IDeclarationBuilderImpl : IDeclarationBuilder, IDeclarationImpl
    {
        Advice ParentAdvice { get; }

        new AttributeBuilderCollection Attributes { get; }
    }
}