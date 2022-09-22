// Warning LAMA0035 on ``: `The aspect layers 'Metalama.Framework.Tests.PublicPipeline.Aspects.Initialize.DuplicateMemberName2.Aspect1' and 'Metalama.Framework.Tests.PublicPipeline.Aspects.Initialize.DuplicateMemberName2.Aspect2' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Error LAMA0036 on `Template`: `The class 'Aspect2' defines a new template named 'Template', but the base class 'Aspect1' already defines a template of the same name. Template names must be unique.`
internal class Target
{
}