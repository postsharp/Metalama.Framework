[Aspect]
internal class Target
{
  class TestType : global::System.Object
  {
    private global::System.Int32 _introducedProperty;
    public global::System.Int32 IntroducedProperty
    {
      get
      {
        global::System.Console.WriteLine("Override");
        return this._introducedProperty;
      }
      set
      {
        global::System.Console.WriteLine("Override");
        this._introducedProperty = value;
      }
    }
  }
}