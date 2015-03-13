using Microsoft.Data.Visualization.VisualizationControls;
using System;
using System.Threading;

namespace Microsoft.Data.Visualization.Client.Excel
{
    public class ExcelGeoDataSource : GeoDataSource
    {
        public Func<ADODB.Connection> ModelConnectionFactory { get; private set; }

        public ExcelGeoDataSource(string name, Func<ADODB.Connection> getConnection)
            : base(name)
        {
            if (getConnection == null)
                throw new ArgumentNullException("getConnection");
            this.ModelConnectionFactory = getConnection;
        }

        protected override DataView InstantiateDataView(DataView.DataViewType type)
        {
            switch (type)
            {
                case DataView.DataViewType.Excel:
                    return new GeoDataView(this);
                case DataView.DataViewType.Chart:
                    return new ChartDataView(this);
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        protected override ModelQuery InstantiateModelQuery(string name, Microsoft.Data.Visualization.VisualizationControls.Filter filter, CancellationToken cancellationToken)
        {
            Func<ADODB.Connection> connectionFactory = this.ModelConnectionFactory;
            if (connectionFactory == null)
                return null;
            return new ExcelModelQuery(name, filter, this.ModelCulture, connectionFactory, cancellationToken);
        }

        protected override void Shutdown()
        {
            if (!this.OkayToShutdown)
                return;
            this.ModelConnectionFactory = null;
            base.Shutdown();
        }
    }
}
