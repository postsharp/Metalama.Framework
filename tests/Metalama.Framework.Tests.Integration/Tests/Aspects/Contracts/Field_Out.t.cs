internal class Target
{


    private global::System.String _q1;


    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Out.NotNullAttribute]
    private global::System.String q
    {
        get
        {
            global::System.String returnValue;
            returnValue = this._q1; if (returnValue == null)
            {
                throw new global::System.ArgumentNullException();
            }

            return returnValue;


        }
        set
        {
            this._q1 = value;
        }
    }
}