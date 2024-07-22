[Memento]
internal class TargetClass : global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.IOriginator
{
  private int _state1;
  private int State2 { get; set; }
  public void Restore(global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.IMemento memento)
  {
    this._state1 = ((global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.TargetClass.Memento)memento)._state1;
    this.State2 = ((global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.TargetClass.Memento)memento).State2;
  }
  public global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.IMemento Save()
  {
    return (global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.IMemento)new global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.TargetClass.Memento(this._state1, this.State2);
  }
  public class Memento : global::Metalama.Framework.Tests.Integration.Aspects.Samples.Memento.IMemento
  {
    public readonly global::System.Int32 State2;
    public readonly global::System.Int32 _state1;
    public Memento(global::System.Int32 _state1, global::System.Int32 State2)
    {
      this._state1 = _state1;
      this.State2 = State2;
    }
  }
}