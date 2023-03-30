// Warning CS8618 on `S`: `Non-nullable field 'S' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
// Warning CS8618 on `ANS`: `Non-nullable field 'ANS' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
[GenerateMethods]
public class Target
{
  public string S;
  public string? [] ANS;
  public string? NS;
  public int I;
  public int? NI;
  public void UseANS()
  {
    global::System.String? []? value = default;
  }
  public void UseI()
  {
    global::System.Int32? value = default;
  }
  public void UseNI()
  {
    global::System.Int32? value = default;
  }
  public void UseNS()
  {
    global::System.String? value = default;
  }
  public void UseS()
  {
    global::System.String? value = default;
  }
}