// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.Engine.Advising;

internal interface IAdviceFactoryImpl : IAdviceFactory
#pragma warning restore CS0612 // Type or member is obsolete
{
    IAdviceFactoryImpl WithTemplateClassInstance( TemplateClassInstance templateClassInstance );
}