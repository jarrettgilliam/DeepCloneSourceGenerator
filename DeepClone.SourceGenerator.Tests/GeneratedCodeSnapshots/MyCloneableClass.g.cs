// <auto-generated/>
namespace DeepClone.SourceGenerator.UnitTests;

public partial class MyCloneableClass
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DeepClone.SourceGenerator", "1.0.0.0")]
    public MyCloneableClass DeepClone() =>
        new MyCloneableClass
        {
            MyInt = this.MyInt,
            MyString = this.MyString,
            MyCloneableClass = this.MyCloneableClass?.DeepClone()
        };
}