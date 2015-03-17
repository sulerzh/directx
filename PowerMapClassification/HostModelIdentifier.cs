using Microsoft.Data.Recommendation.Client;
using Microsoft.Data.Recommendation.Common;
using Microsoft.Data.Visualization.VisualizationControls;
using System;

namespace Microsoft.Data.Recommendation.Client.PowerMap
{
    internal class HostModelIdentifier : IHostModelIdentifier, IEquatable<IHostModelIdentifier>
    {
        public string ObjectName { get; private set; }

        public string ParentObjectName { get; private set; }

        public AnalysisObjectType AnalysisObjectType { get; set; }

        public TableColumn Column { get; set; }

        public HostModelIdentifier(string name, string parent, AnalysisObjectType type)
        {
            this.ObjectName = name;
            this.ParentObjectName = parent;
            this.AnalysisObjectType = type;
        }

        public HostModelIdentifier(string name, string parent, AnalysisObjectType type, TableColumn hostColumn)
            : this(name, parent, type)
        {
            this.Column = hostColumn;
        }

        public bool Equals(IHostModelIdentifier identifier)
        {
            return this == identifier;
        }

        public override string ToString()
        {
            return this.ObjectName;
        }
    }
}
