internal class TestClass
{
  public void Method1([TestContract] string nonNullableString)
  {
    this.Method1_Source(nonNullableString);
    this.Method1_Source(nonNullableString);
  }
  private void Method1_Source(string nonNullableString)
  {
  }
}