// Warning CS8618 on `P`: `Non-nullable property 'P' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`
class B
{
  public virtual string P { get; set; }
}
class C : B
{
  [NotNull]
  public override string P
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
class C2 : C
{
  public override string P
  {
    set
    {
    }
  }
}