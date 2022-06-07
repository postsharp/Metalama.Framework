[MyAspect]
public class BaseClass
{
    public BaseClass(global::System.ICustomFormatter formatter)
    { this.formatter = formatter; }
    public BaseClass(int x, global::System.ICustomFormatter formatter) : this(formatter)
    {

    }

    public BaseClass(long x, global::System.ICustomFormatter formatter)
    { this.formatter = formatter; }

    private global::System.ICustomFormatter formatter;
}

public class DerivedClass : BaseClass
{
    public DerivedClass(global::System.ICustomFormatter formatter)
: base(formatter)
    {

    }

    public DerivedClass(int x, global::System.ICustomFormatter formatter) : this(formatter)
    {

    }

    public DerivedClass(long x, global::System.ICustomFormatter formatter) : base(x, formatter)
    {
    }

    public DerivedClass(float x, global::System.ICustomFormatter formatter) : this((int)x, formatter)
    {

    }
}