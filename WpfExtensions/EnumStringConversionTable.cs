using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class EnumStringConversionTable
  {
    private Dictionary<object, string> _displayStrings = new Dictionary<object, string>();

    public EnumStringConversionTable(Type type)
    {
      foreach (object key in Enum.GetValues(type))
      {
        DisplayStringAttribute displayStringAttribute = (DisplayStringAttribute) type.GetMember(key.ToString())[0].GetCustomAttributes(typeof (DisplayStringAttribute), false)[0];
        this._displayStrings.Add(key, displayStringAttribute.Value);
      }
    }

    public string GetDisplayString(object enumValue)
    {
      return this._displayStrings[enumValue];
    }
  }
}
