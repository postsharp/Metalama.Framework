class TargetCode
{
    [MethodAspect]
    int Method(int a)
    {
        throw new global::System.NotSupportedException();
    }

    private int _property;

        
    // TODO BUg 28882
    // [PropertyAspect]
    // int field;
        
    [PropertyAspect]
    int Property {get    {
            throw new global::System.NotSupportedException();
        }

        set    {
            throw new global::System.NotSupportedException();
        }
    }

    private int _property2;

        
    [PropertyAspect2]
    int Property2 {get    {
            throw new global::System.NotSupportedException();
        }

        set    {
            this._property2=value;    }
    }
}