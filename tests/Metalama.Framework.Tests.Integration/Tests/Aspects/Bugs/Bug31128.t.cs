// Warning CS0628 on `CreateColumns`: `'UsersLoginInfoModel.CreateColumns()': new protected member declared in sealed type`
public sealed partial class UsersLoginInfoModel : BusinessObjectModel<UsersLoginInfo>
{
    public UsersLoginInfoModel()
    {
    }
    public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn Id
    {
        get
        {
            return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn)base.Columns["Id"];
        }
    }
    public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn ProviderUserKey
    {
        get
        {
            return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn)base.Columns["ProviderUserKey"];
        }
    }
    protected global::System.Collections.Generic.IList<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn> CreateColumns()
    {
        global::System.Collections.Generic.IList<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn> columns;
        columns = default(global::System.Collections.Generic.IList<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn>);
        columns.Add(new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn("Id")
        { VisibleInDetailView = false });
        columns.Add(new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn("ProviderUserKey"));
        return (global::System.Collections.Generic.IList<global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31128.BusinessObjectModelColumn>)columns;
    }
}