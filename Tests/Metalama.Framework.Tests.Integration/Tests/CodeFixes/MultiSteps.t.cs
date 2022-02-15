[ToString]
internal class MovingVertex
{
    [NotToString]
    public double X;

    public double Y;

    public double DX;

    public double DY { get; set; }

    public double Velocity => Math.Sqrt((DX * DX) + (DY * DY));


    public override global::System.String ToString()
    {
        throw new global::System.NotImplementedException();
    }
}