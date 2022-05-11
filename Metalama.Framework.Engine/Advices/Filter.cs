// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Advices
{
    internal record Filter( Ref<IDeclaration> TargetDeclaration, TemplateMember<IMethod> Template, FilterDirection Kind, IObjectReader Tags, IObjectReader Args );
}