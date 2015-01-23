using System;
using System.Resources;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public interface IResourceString
  {
    Type ResourceManagerSource { get; set; }

    ResourceManager ResourceManager { get; }

    string ResourceKey { get; set; }

    string ResourceValue { get; }

    bool IsGeneratedResourceType();

    void ValidateResourceDescription();
  }
}
