[Aspect]
class Target : ICloneable
{
  int _field;
  int Property { get; set; }
  int this[int i] => 42;
  event EventHandler? Event;
  public static Target operator -(Target target) => target;
  public object Clone() => new Target();
  ~Target()
  {
  }
}