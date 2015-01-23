using System;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  [AttributeUsage(AttributeTargets.Property)]
  public abstract class ResourceStringAttribute : Attribute, IResourceString
  {
    public const string ResGeneratedResourceManagerPropertyName = "ResourceManager";
    private Type _type;
    private string _resourceKey;
    private PropertyInfo _resourceManagerProperty;

    public string ResourceKey
    {
      get
      {
        return this._resourceKey;
      }
      set
      {
        if (!(this._resourceKey != value))
          return;
        if (string.IsNullOrEmpty(value))
          throw new ArgumentException("resource key cannot be null or empty.");
        this._resourceKey = value;
      }
    }

    public ResourceManager ResourceManager
    {
      get
      {
        return this.ResourceManagerProperty.GetValue((object) null, (object[]) null) as ResourceManager;
      }
    }

    private PropertyInfo ResourceManagerProperty
    {
      get
      {
        this.RefreshResourceManagerProperty();
        return this._resourceManagerProperty;
      }
    }

    public Type ResourceManagerSource
    {
      get
      {
        return this._type;
      }
      set
      {
        if (!(this._type != value))
          return;
        this._type = value;
        this._resourceManagerProperty = (PropertyInfo) null;
        this.RefreshResourceManagerProperty();
      }
    }

    public string ResourceValue
    {
      get
      {
        return this.ResourceManager.GetString(this.ResourceKey);
      }
    }

    public static ResourceStringAttribute Parse(ICustomAttributeProvider decoratedItem)
    {
      if (decoratedItem == null)
        throw new ArgumentNullException("decoratedItem");
      object[] customAttributes = decoratedItem.GetCustomAttributes(typeof (ResourceStringAttribute), false);
      if (customAttributes.Length < 1)
        return (ResourceStringAttribute) null;
      else
        return customAttributes[0] as ResourceStringAttribute;
    }

    public bool IsGeneratedResourceType()
    {
      if ((Type) null == this.ResourceManagerSource || (PropertyInfo) null == this.ResourceManagerProperty)
        return false;
      else
        return this.ResourceManagerProperty.PropertyType == typeof (ResourceManager);
    }

    private void RefreshResourceManagerProperty()
    {
      if (!((PropertyInfo) null == this._resourceManagerProperty))
        return;
      this._resourceManagerProperty = this.ResourceManagerSource.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    }

    public void ValidateResourceDescription()
    {
      if (string.IsNullOrEmpty(this.ResourceKey))
        throw new ArgumentException("ResourceKey cannot be null or empty.");
      if ((Type) null == this.ResourceManagerSource)
        throw new ArgumentException("type must be decorated with ResourceSourceAttribute");
      if (!this.IsGeneratedResourceType())
        throw new ArgumentException("type's ResourceSourceAttribute did not specify a valid resource type");
      if (this.ResourceManager.GetString(this.ResourceKey) != null)
        return;
      throw new MissingManifestResourceException(string.Format((IFormatProvider) Thread.CurrentThread.CurrentUICulture, "the resource key {0} does not resolve to a string in {1}", new object[2]
      {
        (object) this.ResourceKey,
        (object) this.ResourceManagerSource
      }));
    }
  }
}
