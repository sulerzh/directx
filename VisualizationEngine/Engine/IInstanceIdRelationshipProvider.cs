using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    public interface IInstanceIdRelationshipProvider
    {
        IEnumerable<InstanceId> GetRelatedIdsOverTime(InstanceId id);
    }
}
