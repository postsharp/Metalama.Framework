
// --- NamespaceLimitedRedistributionAllowed.cs ---

// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect1' and 'RedistributionTests.TargetNamespace.RedistributionAspect2' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect1' and 'RedistributionTests.TargetNamespace.RedistributionAspect2' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect2' and 'RedistributionTests.TargetNamespace.RedistributionAspect3' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect2' and 'RedistributionTests.TargetNamespace.RedistributionAspect3' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect3' and 'RedistributionTests.TargetNamespace.RedistributionAspect4' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect3' and 'RedistributionTests.TargetNamespace.RedistributionAspect4' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.NamespaceLimitedRedistributionAllowed;

class Dummy
{
}

// --- _Redistribution.cs ---

// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect1' and 'RedistributionTests.TargetNamespace.RedistributionAspect2' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect1' and 'RedistributionTests.TargetNamespace.RedistributionAspect2' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect2' and 'RedistributionTests.TargetNamespace.RedistributionAspect3' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect2' and 'RedistributionTests.TargetNamespace.RedistributionAspect3' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect3' and 'RedistributionTests.TargetNamespace.RedistributionAspect4' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
// Warning LAMA0035 on ``: `The aspect layers 'RedistributionTests.TargetNamespace.RedistributionAspect3' and 'RedistributionTests.TargetNamespace.RedistributionAspect4' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
using RedistributionTests.TargetNamespace;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution;

class RedistributionTargetClass
{
    [RedistributionAspect1]
    [RedistributionAspect2]
    [RedistributionAspect3]
    [RedistributionAspect4]
    void RedistributionTargetMethod()
    {
    global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect4");
        global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect3");
        global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect2");
        global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect1");
        goto __aspect_return_3;

__aspect_return_3:    goto __aspect_return_2;

__aspect_return_2:    goto __aspect_return_1;

__aspect_return_1:    return;
    }
}
