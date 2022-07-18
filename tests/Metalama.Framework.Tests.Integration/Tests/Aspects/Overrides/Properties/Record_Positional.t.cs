internal record MyRecord(int A, int B)
{

    private int _a = A;
    public int A
    {
        get
        {
            return this._a;
        }
        init
        {
            this._a = value;
        }
    }

    private int _b = B;
    public int B
    {
        get
        {
            return this._b;
        }
        init
        {
            this._b = value;
        }
    }
}