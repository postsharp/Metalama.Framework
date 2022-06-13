[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Signatures.IInterface
{

    public T? GenericMethod<T>(T? x)
    {
        return (T?)x;
    }

    public T? GenericMethod_DoubleNestedParam<T>(global::System.Collections.Generic.List<global::System.Collections.Generic.List<T>> x)
    {
        if (x.Count > 0)
        {
            if (x[0].Count > 0)
            {
                return (T?)x[0][0];
            }
            else
            {
                return (T?)default(T?);
            }
        }
        else
        {
            return (T?)default(T?);
        }
    }

    public T? GenericMethod_Multiple<T, U>(T? x, U? y)
    {
        return (T?)x;
    }

    public T? GenericMethod_MultipleReverse<T, U>(U? x, T? y)
    {
        return (T?)y;
    }

    public T? GenericMethod_NestedParam<T>(global::System.Collections.Generic.List<T> x)
    {
        if (x.Count > 0)
        {
            return (T?)x[0];
        }
        else
        {
            return (T?)default(T?);
        }
    }

    public void GenericMethod_Out<T>(out T? x)
    {
        x = default(T?);
    }

    public T? GenericMethod_Ref<T>(ref T? x)
    {
        return (T?)x;
    }

    public global::System.Int32 Method(global::System.Int32 x, global::System.String y)
    {
        return (global::System.Int32)x;
    }

    public global::System.Int32 Method_Ref(ref global::System.Int32 x)
    {
        return (global::System.Int32)x;
    }

    public void VoidMethod()
    {
    }
}