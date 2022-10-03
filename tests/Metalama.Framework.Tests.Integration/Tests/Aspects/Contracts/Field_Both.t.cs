internal class Target
{
  private global::System.String _q1;
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Both.NotNullAttribute]
  private global::System.String q
  {
    get
    {
      global::System.String returnValue;
      returnValue = this._q1;
      if (returnValue == null)
      {
        throw new global::System.ArgumentNullException();
      }
      return returnValue;
    }
    set
    {
      if (value == null)
      {
        throw new global::System.ArgumentNullException();
      }
      this._q1 = value;
    }
  }
}
internal struct TargetStruct
{
  private global::System.String _q1 = default;
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Both.NotNullAttribute]
  private global::System.String q
  {
    get
    {
      global::System.String returnValue;
      returnValue = this._q1;
      if (returnValue == null)
      {
        throw new global::System.ArgumentNullException();
      }
      return returnValue;
    }
    set
    {
      if (value == null)
      {
        throw new global::System.ArgumentNullException();
      }
      this._q1 = value;
    }
  }
  public TargetStruct(string q)
  {
    this.q = q;
  }
}