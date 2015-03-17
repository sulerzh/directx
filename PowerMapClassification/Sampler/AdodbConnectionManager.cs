using ADODB;
using System;

namespace Microsoft.Data.Recommendation.Client.PowerMap.Sampler
{
    internal class AdodbConnectionManager : IDisposable
    {
        private Connection Connection { get; set; }

        public AdodbConnectionManager(Connection connection)
        {
            this.Connection = connection;
        }

        public Connection GetConnection()
        {
            int state = this.Connection.State;
            return this.Connection;
        }

        public void Dispose()
        {
        }
    }
}
