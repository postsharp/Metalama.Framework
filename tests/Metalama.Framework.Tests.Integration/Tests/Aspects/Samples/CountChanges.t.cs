// Final Compilation.Emit failed.
// Error CS1061 on `_addressChangeCount`: `'C' does not contain a definition for '_addressChangeCount' and no accessible extension method '_addressChangeCount' accepting a first argument of type 'C' could be found (are you missing a using directive or an assembly reference?)`
// Error CS1061 on `_addressChangeCount`: `'C' does not contain a definition for '_addressChangeCount' and no accessible extension method '_addressChangeCount' accepting a first argument of type 'C' could be found (are you missing a using directive or an assembly reference?)`
// Error CS1061 on `_itemsChangeCount`: `'C' does not contain a definition for '_itemsChangeCount' and no accessible extension method '_itemsChangeCount' accepting a first argument of type 'C' could be found (are you missing a using directive or an assembly reference?)`
// Error CS1061 on `_itemsChangeCount`: `'C' does not contain a definition for '_itemsChangeCount' and no accessible extension method '_itemsChangeCount' accepting a first argument of type 'C' could be found (are you missing a using directive or an assembly reference?)`
// Error CS1061 on `_addressChangeCount`: `'C' does not contain a definition for '_addressChangeCount' and no accessible extension method '_addressChangeCount' accepting a first argument of type 'C' could be found (are you missing a using directive or an assembly reference?)`
// Error CS1061 on `_itemsChangeCount`: `'C' does not contain a definition for '_itemsChangeCount' and no accessible extension method '_itemsChangeCount' accepting a first argument of type 'C' could be found (are you missing a using directive or an assembly reference?)`
[CountChanges]
class C
{
    private string? _address;
    public string? Address
    {
        get
        {
            return this._address;
        }
        set
        {
            var oldValue = this._address;
            this._address = value;
            if (oldValue != this._address)
            {
                this._addressChangeCount = this._addressChangeCount + 1;
            }
        }
    }
    private int _items;
    public int Items
    {
        get
        {
            return this._items;
        }
        set
        {
            var oldValue = this._items;
            this._items = value;
            if (oldValue != this._items)
            {
                this._itemsChangeCount = this._itemsChangeCount + 1;
            }
        }
    }
    public global::System.Int32 AddressChangeCount { get; set; }
    public global::System.Int32 ItemsChangeCount { get; set; }
    public global::System.Int32 TotalChanges
    {
        get
        {
            int sum = 0;
            sum += this._addressChangeCount;
            sum += this._itemsChangeCount;
            return (global::System.Int32)sum;
        }
    }
}