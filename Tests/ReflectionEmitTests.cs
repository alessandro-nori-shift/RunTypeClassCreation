using System.Reflection;
using DynamicClassCreation.ReflectionEmit;

namespace Tests;

[TestFixture]
public class ReflectionEmitTests
{
    [Test]
    public void NewTypeBuilderTest()
    {
        const string myNewTypeName = "MyNetType";
        const string myIntPropertyName = "MyIntProperty";
        const int myIntPropertyExpectedValue = 5;
        const string myHashSetPropertyName = "MyHashSetProperty";
        var myHashSetPropertyExpectedValue = new HashSet<long> { 1, 2, 3, 4, 5 };

        var myTypeBuilder = new MyTypeBuilder(myNewTypeName);
        myTypeBuilder.AddProperty(typeof(int), myIntPropertyName);
        myTypeBuilder.AddProperty(typeof(HashSet<long>), myHashSetPropertyName);

        var myNewType = myTypeBuilder.Compile();

        var myNewInstance = Activator.CreateInstance(myNewType);

        myNewType.InvokeMember(myIntPropertyName, BindingFlags.SetProperty, null, myNewInstance,
            new object[] { myIntPropertyExpectedValue });
        var myIntPropertyValue = (int)myNewType.InvokeMember(myIntPropertyName, BindingFlags.GetProperty, null,
            myNewInstance, Array.Empty<object>())!;

        myNewType.InvokeMember(myHashSetPropertyName, BindingFlags.SetProperty, null, myNewInstance,
            new object[] { myHashSetPropertyExpectedValue });
        var myHashSetPropertyValue = (HashSet<long>)myNewType.InvokeMember(myHashSetPropertyName,
            BindingFlags.GetProperty, null, myNewInstance, Array.Empty<object>())!;

        Assert.Multiple(() =>
        {
            Assert.That(myIntPropertyValue, Is.EqualTo(myIntPropertyExpectedValue));
            Assert.That(myHashSetPropertyValue, Is.EqualTo(myHashSetPropertyExpectedValue));
        });
    }
}