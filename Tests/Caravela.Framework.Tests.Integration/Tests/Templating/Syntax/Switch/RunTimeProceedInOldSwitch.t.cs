int Method(int a)
{
    int i = 1;
    switch (i)
    {
        case 0:
            global::System.Console.WriteLine("0");
            break;
        case 1:
        {
            global::System.Int32 x;
            x = this.Method(a);
        }

            break;
    }

    return this.Method(a);
}