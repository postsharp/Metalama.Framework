[MyAspect]
public class BaseClass
{


    private global::System.ICustomFormatter formatter;

    public BaseClass(global::System.ICustomFormatter formatter)
    { this.formatter = formatter; }
}

public class DerivedClass : BaseClass
{


    public DerivedClass(global::System.ICustomFormatter formatter)
    : base(formatter)
    {
    }
}