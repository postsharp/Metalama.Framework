class TargetCode
    {
        [Import]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        IFormatProvider FormatProvider {get    {
        return (global::System.IFormatProvider)(global::Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug28973.ServiceLocator.ServiceProvider.GetService(typeof(global::System.IFormatProvider)));
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }