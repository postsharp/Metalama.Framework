// Warning CS8618 on `_serviceProvider`: `Non-nullable field '_serviceProvider' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
public class Commerce
{
    [Inject]
    private IDisposable _BillingProcessor;

    [Inject]
    private IDisposable _CustomerProcessor;

    [Inject]
    private IDisposable _Notifier;


private global::System.IServiceProvider _serviceProvider;}
