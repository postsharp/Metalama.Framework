class TargetCode
{


    private global::System.IFormatProvider _formatProvider;


    [global::Metalama.Framework.Tests.Integration.Aspects.Bugs.Bug28973.ImportAttribute]
    private global::System.IFormatProvider FormatProvider
    {
        get
        {
            return (global::System.IFormatProvider)global::Metalama.Framework.Tests.Integration.Aspects.Bugs.Bug28973.ServiceLocator.ServiceProvider.GetService(typeof(global::System.IFormatProvider));

        }
        set
        {
            this._formatProvider = value;
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}