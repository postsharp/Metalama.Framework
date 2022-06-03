[Introduction]
internal class TargetClass
{

    private global::System.Int32 _autoPropertyWithRestrictedGet;


    public global::System.Int32 AutoPropertyWithRestrictedGet
    {
        private get
        {
            return this._autoPropertyWithRestrictedGet;
        }
        set
        {
            this._autoPropertyWithRestrictedGet = value;
        }
    }

    private global::System.Int32 _autoPropertyWithRestrictedInit;


    public global::System.Int32 AutoPropertyWithRestrictedInit
    {
        get
        {
            return this._autoPropertyWithRestrictedInit;
        }
        private init
        {
            this._autoPropertyWithRestrictedInit = value;
        }
    }

    private global::System.Int32 _autoPropertyWithRestrictedSet;


    public global::System.Int32 AutoPropertyWithRestrictedSet
    {
        get
        {
            return this._autoPropertyWithRestrictedSet;
        }
        private set
        {
            this._autoPropertyWithRestrictedSet = value;
        }
    }

    public global::System.Int32 PropertyWithRestrictedGet
    {
        private get
        {
            return (global::System.Int32)42;
        }

        set
        {
        }
    }

    public global::System.Int32 PropertyWithRestrictedInit
    {
        get
        {
            return (global::System.Int32)42;
        }

        private init
        {
        }
    }

    public global::System.Int32 PropertyWithRestrictedSet
    {
        get
        {
            return (global::System.Int32)42;
        }

        private set
        {
        }
    }

    private global::System.Int32 _protectedInternalPropertyWithProtectedSetter;


    protected internal global::System.Int32 ProtectedInternalPropertyWithProtectedSetter
    {
        get
        {
            return this._protectedInternalPropertyWithProtectedSetter;
        }
        protected set
        {
            this._protectedInternalPropertyWithProtectedSetter = value;
        }
    }

    private global::System.Int32 _protectedPropertyWithPrivateProtectedSetter;


    protected global::System.Int32 ProtectedPropertyWithPrivateProtectedSetter
    {
        get
        {
            return this._protectedPropertyWithPrivateProtectedSetter;
        }
        private protected set
        {
            this._protectedPropertyWithPrivateProtectedSetter = value;
        }
    }
}