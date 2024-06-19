[CountChanges]
internal class C
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
        this.AddressChangeCount = this.AddressChangeCount + 1;
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
        this.ItemsChangeCount = this.ItemsChangeCount + 1;
      }
    }
  }
  public global::System.Int32 AddressChangeCount { get; set; }
  public global::System.Int32 ItemsChangeCount { get; set; }
  public global::System.Int32 TotalChanges
  {
    get
    {
      var sum = 0;
      sum += this.AddressChangeCount;
      sum += this.ItemsChangeCount;
      return (global::System.Int32)sum;
    }
  }
}