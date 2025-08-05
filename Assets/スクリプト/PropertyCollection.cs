using System.Collections;
using System.Collections.Generic;

public class PropertyArray<PropertyType> : IProperty<PropertyType>
{
    readonly PropertyType[] propertyArray;
    public PropertyType this[int index]
    {
        set { Set(index, value); }
        get => Get(index);
    }
    public PropertyArray(int size)
    {
        propertyArray = new PropertyType[size];
    }
    public virtual void Set(int index, PropertyType value)
    {
        propertyArray[index] = value;
    }
    public virtual PropertyType Get(int index)
    {
        return propertyArray[index];
    }
}

public interface IProperty<PropertyType>
{
    virtual void Set(int index, PropertyType value) { }
    PropertyType Get(int index);
}