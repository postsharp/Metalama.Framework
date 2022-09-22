// Warning LAMA0035 on ``: `The aspect layers 'Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UnorderedAspects.Aspect1' and 'Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UnorderedAspects.Aspect2' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
public class T
{
  [Aspect1]
  [Aspect2]
  public void M()
  {
    global::System.Console.WriteLine("Aspect2");
    return;
  }
}