// Warning CS8618 on `Event1`: `Non-nullable event 'Event1' must contain a non-null value when exiting constructor. Consider declaring the event as nullable.`
// Warning CS8618 on `Event2`: `Non-nullable event 'Event2' must contain a non-null value when exiting constructor. Consider declaring the event as nullable.`
// Warning CS8618 on `Event3`: `Non-nullable event 'Event3' must contain a non-null value when exiting constructor. Consider declaring the event as nullable.`
internal class C
{
  [MyAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]
  private event Action Event1;
  [MyAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]
  private event Action Event2;
  [MyAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]
  private event Action Event3;
  [MyAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]
  private event Action Event4
  {
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]
    add
    {
    }
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]
    remove
    {
    }
  }
}