[NotifyPropertyChanged]
internal partial class Person:global::System.ComponentModel.INotifyPropertyChanged{
    #region Constructors

    public Person() { }

    #endregion

    #region Public Properties

    public string FirstName 
{ get
{ 
        return this.FirstName_Source;

}
set
{ 
        if (value != this.FirstName_Source)
        {
            this.FirstName_Source = value;
            OnPropertyChanged("FirstName");
        }

        return;

}
}

private string FirstName_Source { get; set; }

    public string FullName
    {
        get
        {
            return $"{FirstName} {LastName}";
        }
    }

    public string LastName 
{ get
{ 
        return this.LastName_Source;

}
set
{ 
        if (value != this.LastName_Source)
        {
            this.LastName_Source = value;
            OnPropertyChanged("LastName");
        }

        return;

}
}

private string LastName_Source { get; set; }


protected void OnPropertyChanged(global::System.String name)
{
    PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
}

public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    #endregion
}

[OptionalValueType]
public partial class Account
{
    #region Public Properties

    public string? Name 
{ get
{ 
        return (global::System.String? )((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional)this.OptionalValues).Name.Value;

}
set
{ 
        ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional)this.OptionalValues).Name = new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.OptionalValue<global::System.String?>(value);

}
}

    public Account? Parent 
{ get
{ 
        return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account? )((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional)this.OptionalValues).Parent.Value;

}
set
{ 
        ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional)this.OptionalValues).Parent = new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.OptionalValue<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account?>(value);

}
}

    #endregion

    public partial class Optional { 

public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.OptionalValue<global::System.String?> Name { get; set; }

public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.OptionalValue<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional> OptionalValues { get; set; }

public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.OptionalValue<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account?> Parent { get; set; }}


public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional OptionalValues 
{ get
{ 
        return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional)((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional)this.OptionalValues).OptionalValues.Value;

}
set
{ 
        ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional)this.OptionalValues).OptionalValues = new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.OptionalValue<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355.Account.Optional>(value);

}
} }