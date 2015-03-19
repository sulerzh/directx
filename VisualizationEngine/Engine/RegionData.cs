using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
    [DataContract(Name = "rdata", Namespace = "")]
    public class RegionData
    {
        private const string SafeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
        private string ring;

        public List<Coordinates> Polygon { get; private set; }

        [DataMember(Name = "id", Order = 1)]
        public long PolygonId { get; set; }

        [DataMember(Name = "ring", Order = 2)]
        public string Ring
        {
            get
            {
                return this.ring;
            }
            set
            {
                this.ring = value;
                List<Coordinates> parsedValue;
                if (!string.IsNullOrWhiteSpace(this.ring) && RegionData.TryParseEncodedValue(this.ring, out parsedValue))
                {
                    this.Polygon = parsedValue;
                }
                else
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Failed to parse compression region polygon for Polygon ID {0}", (object)this.PolygonId);
                    this.Polygon = new List<Coordinates>();
                }
            }
        }

        /// <summary>
        /// 从压缩后的字符串中解析坐标串
        /// 压缩算法见：https://msdn.microsoft.com/en-us/library/jj158958.aspx
        /// 解析算法见：http://blogs.bing.com/maps/2013/06/25/retrieving-boundaries-from-the-bing-spatial-data-services-preview/
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parsedValue"></param>
        /// <returns></returns>
        private static bool TryParseEncodedValue(string value, out List<Coordinates> parsedValue)
        {
            parsedValue = null;
            var list = new List<Coordinates>();
            var index = 0;
            var xsum = 0;
            var ysum = 0;

            while (index < value.Length)
            {
                var n = 0;
                var k = 0;

                while (true)
                {
                    if (index >= value.Length)
                    {
                        return false;
                    }
                    var b = SafeCharacters.IndexOf(value[index++]);
                    if (b == -1)
                    {
                        return false;
                    }

                    n |= (b & 31) << k;
                    k += 5;
                    if (b < 32) break;
                }

                var diagonal = (int) ((Math.Sqrt(8*n + 5) - 1)/2);
                n -= diagonal*(diagonal + 1)/2;
                var ny = (int) (n);
                var nx = diagonal - ny;
                nx = (nx >> 1) ^ -(nx & 1);
                ny = (ny >> 1) ^ -(ny & 1);
                xsum += nx;
                ysum += ny;
                var lat = ysum*0.00001;
                var lon = xsum*0.00001;
                list.Add(Coordinates.FromDegrees(lat, lon));
            }
            parsedValue = list;
            return true;
        }
    }
}
