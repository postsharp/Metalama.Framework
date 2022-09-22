internal class TargetCode
{
  internal class MovingVertex
  {
    public double X;
    public double Y;
    public double DX;
    public double DY { get; set; }
    public double Velocity => Math.Sqrt((DX * DX) + (DY * DY));
    public override string ToString()
    {
      throw new NotImplementedException();
    }
  }
}