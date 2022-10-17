class Yack
{
  [Import]
  IGreetingService? _service
  {
    get
    {
      var value = this._service_Source;
      if (value == null)
      {
        value = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService? )global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.ServiceLocator.ServiceProvider.GetService(typeof(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService)));
        this._service_Source = value ?? throw new global::System.InvalidOperationException("Cannot get a service of type IGreetingService?.");
      }
      return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService? )value;
    }
    set
    {
      this._service_Source = value;
    }
  }
  private IGreetingService? _service_Source { get; set; }
}