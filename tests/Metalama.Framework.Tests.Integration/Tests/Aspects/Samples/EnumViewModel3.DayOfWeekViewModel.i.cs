namespace Doc.EnumViewModel
{
  class DayOfWeekViewModel
  {
    private readonly global::System.DayOfWeek _value;
    public DayOfWeekViewModel(global::System.DayOfWeek underlying)
    {
      this._value = underlying;
    }
    public global::System.Boolean IsFriday
    {
      get
      {
        return (global::System.Boolean)(this._value == global::System.DayOfWeek.Friday);
      }
    }
    public global::System.Boolean IsMonday
    {
      get
      {
        return (global::System.Boolean)(this._value == global::System.DayOfWeek.Monday);
      }
    }
    public global::System.Boolean IsSaturday
    {
      get
      {
        return (global::System.Boolean)(this._value == global::System.DayOfWeek.Saturday);
      }
    }
    public global::System.Boolean IsSunday
    {
      get
      {
        return (global::System.Boolean)(this._value == global::System.DayOfWeek.Sunday);
      }
    }
    public global::System.Boolean IsThursday
    {
      get
      {
        return (global::System.Boolean)(this._value == global::System.DayOfWeek.Thursday);
      }
    }
    public global::System.Boolean IsTuesday
    {
      get
      {
        return (global::System.Boolean)(this._value == global::System.DayOfWeek.Tuesday);
      }
    }
    public global::System.Boolean IsWednesday
    {
      get
      {
        return (global::System.Boolean)(this._value == global::System.DayOfWeek.Wednesday);
      }
    }
  }
}