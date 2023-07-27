using System.Reflection;
using DynamicClassCreation.Components;
using RoslynClassCreation.RoslynSyntaxTree;

namespace Tests;

[TestFixture]
public class RoslynTypeBuilderTests
{
    [Test]
    public void RoslynClassCreationTest()
    {
        const string myIntPropertyName = "MyIntProperty";
        const int myIntPropertyExpectedValue = -3;

        var property1 = new Property(myIntPropertyName, typeof(int));
        var property2 = new Property("MyStringProperty", typeof(string));

        var typeBuilder = new RoslynTypeBuilder("MyNewClassName", property1, property2);

        var myNewType = typeBuilder.Compile();

        Assert.That(myNewType, Is.Not.Null);

        var myNewInstance = Activator.CreateInstance(myNewType!);

        Assert.That(myNewInstance, Is.Not.Null);

        myNewType!.InvokeMember(myIntPropertyName, BindingFlags.SetProperty, null, myNewInstance,
            new object[] { myIntPropertyExpectedValue });
        var myIntPropertyValue = (int)myNewType.InvokeMember(myIntPropertyName, BindingFlags.GetProperty, null,
            myNewInstance, Array.Empty<object>())!;

        Assert.That(myIntPropertyValue, Is.EqualTo(myIntPropertyExpectedValue));
    }
}