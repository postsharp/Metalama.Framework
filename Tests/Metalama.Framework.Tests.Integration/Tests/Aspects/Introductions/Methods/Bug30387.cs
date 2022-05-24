using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug30387;

internal class InjectAttribute : FieldOrPropertyAspect
{
    [Introduce( WhenExists = OverrideStrategy.Ignore )]
    private readonly IServiceProvider _serviceProvider;
}

// <target>
public class Commerce
{
    [Inject]
    private IDisposable _BillingProcessor;

    [Inject]
    private IDisposable _CustomerProcessor;

    [Inject]
    private IDisposable _Notifier;
}