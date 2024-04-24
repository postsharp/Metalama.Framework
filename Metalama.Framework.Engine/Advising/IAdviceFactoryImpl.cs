using Metalama.Framework.Advising;
using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.Engine.Advising;

internal interface IAdviceFactoryImpl : IAdviceFactory, IAdviceFactoryInternal
#pragma warning restore CS0612 // Type or member is obsolete
{
    IAdviceFactoryImpl WithTemplateClassInstance( TemplateClassInstance templateClassInstance );
}