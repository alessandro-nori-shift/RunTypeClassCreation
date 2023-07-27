using System;

namespace DynamicClassCreation.Components;

public class Property
{
    public string Name { get; }
    public string FieldName { get; }
    public Type ValueType { get; }

    public Property(string name, Type valueType)
    {
        if (!IsValidName(name))
            throw new ArgumentException("Name must start with a capital letter");

        Name = name;
        FieldName = ToFieldName(name);
        ValueType = valueType;
    }

    private static bool IsValidName(string name)
    {
        if (name.Length == 0)
            return false;

        if (char.IsLower(name, 0))
            return false;

        return true;
    }

    private static string ToFieldName(string propertyName)
    {
        return "_" + char.ToLower(propertyName[0]);
    }

    public override string ToString() => $"{ValueType} {Name} {{ get; set; }}";
}