// Warning CS8601 on `service`: `Possible null reference assignment.`
class TargetCode
    {


private IFormatProvider _formatProvider;
        [Import]
        IFormatProvider FormatProvider {get    {
        return (System.IFormatProvider)global::Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug28973.ServiceLocator.ServiceProvider.GetService(typeof(global::System.IFormatProvider));
    }
}
    }
