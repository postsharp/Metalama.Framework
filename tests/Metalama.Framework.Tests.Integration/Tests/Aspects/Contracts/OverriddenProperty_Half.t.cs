// Warning CS8618 on `Default`: `Non-nullable property 'Default' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`
class C : B
{
  public override string Default => "C";
  public override string Both
  {
    get
    {
      global::System.String returnValue;
      returnValue = "C";
      if (returnValue == null)
      {
        throw new global::System.ArgumentNullException();
      }
      return returnValue;
    }
  }
  public override string Input => "C";
  public override string Output
  {
    get
    {
      global::System.String returnValue;
      returnValue = "C";
      if (returnValue == null)
      {
        throw new global::System.ArgumentNullException();
      }
      return returnValue;
    }
  }
}