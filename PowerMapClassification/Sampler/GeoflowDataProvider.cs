using ADODB;
using Microsoft.Data.Recommendation.Client.Sampler;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Recommendation.Client.PowerMap.Sampler
{
    public class GeoflowDataProvider : IDataProvider
    {
        private AdodbConnectionManager ConnectionManager { get; set; }

        public GeoflowDataProvider(Connection connection)
        {
            this.ConnectionManager = new AdodbConnectionManager(connection);
        }

        public GeoflowDataProvider(string dataSourceIdentity)
        {
        }

        public List<object> ExecuteQuery(string queryStr)
        {
            return this.ExecuteQuery(queryStr, -1);
        }

        public List<object> ExecuteQuery(string queryStr, int maxValueLength)
        {
            object RecordsAffected = 0;
            Recordset recordset = this.ConnectionManager.GetConnection().Execute(queryStr, out RecordsAffected, -1);
            DataTypeEnum type = recordset.Fields[0].Type;
            List<object> list = new List<object>();
            if (!recordset.BOF && !recordset.EOF)
                recordset.MoveFirst();
            while (!recordset.EOF)
            {
                object sampleValue = recordset.Fields[0].Value;
                object obj = this.ProcessSample(type, maxValueLength, sampleValue);
                list.Add(obj);
                recordset.MoveNext();
            }
            recordset.Close();
            return list;
        }

        public object ProcessSample(DataTypeEnum columnType, int maxValueLength, object sampleValue)
        {
            if (maxValueLength > 0 && sampleValue != null)
            {
                string str = this.Sanitize<string>(sampleValue, Convert.ToString, string.Empty);
                if (str.Length > maxValueLength)
                    sampleValue = str.Substring(0, maxValueLength);
            }
            return sampleValue;
        }

        private T Sanitize<T>(object value, Func<object, T> converter, T defaultValue)
        {
            if (value is T)
                return (T)value;
            if (value != null)
            {
                if (!Convert.IsDBNull(value))
                {
                    try
                    {
                        return converter(value);
                    }
                    catch (FormatException ex)
                    {
                        return defaultValue;
                    }
                    catch (InvalidCastException ex)
                    {
                        return defaultValue;
                    }
                    catch (OverflowException ex)
                    {
                        return defaultValue;
                    }
                }
            }
            return defaultValue;
        }
    }
}
