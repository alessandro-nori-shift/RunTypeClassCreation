using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicClassCreation.ReflectionEmit;

public sealed class MyTypeBuilder
{
    private readonly TypeBuilder _typeBuilder;
    private Type? _resultType;

    public MyTypeBuilder(string typeSignature)
    {
        var an = new AssemblyName(typeSignature);

        if (an.Name is null)
            throw new ApplicationException($"{nameof(an.Name)} cannot be null");

        var ab = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);

        var md = ab.DefineDynamicModule(an.Name);

        _typeBuilder = md.DefineType(typeSignature, TypeAttributes.Public | TypeAttributes.Class);
    }

    public void AddProperty(Type propertyType, string propertyName)
    {
        var fieldBuilder = _typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

        var propertyBuilder =
            _typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

        const MethodAttributes getSetAttr =
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        var getPropertyBuilder = _typeBuilder.DefineMethod("get_" + propertyName,
            getSetAttr, propertyType,
            Type.EmptyTypes);

        var getIl = getPropertyBuilder.GetILGenerator();
        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);

        var setPropertyBuilder =
            _typeBuilder.DefineMethod("set_" + propertyName,
                getSetAttr,
                null, new[] { propertyType });

        var setIl = setPropertyBuilder.GetILGenerator();
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);
        setIl.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getPropertyBuilder);
        propertyBuilder.SetSetMethod(setPropertyBuilder);
    }

    public Type Compile()
    {
        return _resultType ??= _typeBuilder.CreateType() ?? throw new ApplicationException();
    }
}