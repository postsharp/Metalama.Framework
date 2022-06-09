internal partial class Greeter
    {


[global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.InjectionSample.InjectAttribute]
private global::System.IO.TextWriter _console 
{ get
{ 
        var value = this._console_Source;
        if (value == null)
        {
            value = (global::System.IO.TextWriter? )this._serviceProvider.GetService(typeof(global::System.IO.TextWriter));
            this._console_Source = value ?? throw new global::System.InvalidOperationException($"Cannot get a service of type System.IO.TextWriter.");
        }

        return (global::System.IO.TextWriter)value;

}
set
{ 
        this._console_Source = value;

}
}
private global::System.IO.TextWriter _console_Source { get; set; }

        public void Greet() => this._console.WriteLine("Hello, world.");


private readonly global::System.IServiceProvider _serviceProvider = (global::System.IServiceProvider)global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.InjectionSample.ServiceLocator.Current;
    }