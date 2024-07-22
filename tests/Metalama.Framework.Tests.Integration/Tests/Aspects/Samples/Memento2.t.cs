[Memento]
public class Vehicle
{
  public string Name { get; }
  public decimal Payload { get; set; }
  public string Fuel { get; set; }
  public Vehicle(string name, decimal payload, string fuel)
  {
    Name = name;
    Payload = payload;
    Fuel = fuel;
  }
  public void Restore(global::System.Object snapshot)
  {
    this.Payload = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Memento2.Vehicle.Snapshot)snapshot).Payload;
    this.Fuel = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Memento2.Vehicle.Snapshot)snapshot).Fuel;
  }
  public global::System.Object Save()
  {
    return new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Memento2.Vehicle.Snapshot(this.Payload, this.Fuel);
  }
  private class Snapshot
  {
    public readonly global::System.String Fuel;
    public readonly global::System.Decimal Payload;
    public Snapshot(global::System.Decimal Payload, global::System.String Fuel)
    {
      this.Payload = Payload;
      this.Fuel = Fuel;
    }
  }
}