// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CameraSnapshot
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
    public class CameraSnapshot : IEquatable<CameraSnapshot>
    {
        private const double TimeZoneInRadians = 0.261799387799149;
        private double longitude;

        [XmlIgnore]
        public double Latitude { get; set; }

        [XmlElement("Latitude")]
        public double LatitudeInDegrees
        {
            get
            {
                return this.Latitude * Constants.DegreesPerRadian;
            }
            set
            {
                this.Latitude = value * (Math.PI / 180.0);
            }
        }

        [XmlIgnore]
        public double Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.longitude = MathEx.GetNormalized(value, Math.PI);
            }
        }

        [XmlElement("Longitude")]
        public double LongitudeInDegrees
        {
            get
            {
                return this.Longitude * Constants.DegreesPerRadian;
            }
            set
            {
                this.Longitude = value * (Math.PI / 180.0);
            }
        }

        public double Rotation { get; set; }

        public double PivotAngle { get; set; }

        public double Distance { get; set; }

        public double Altitude
        {
            get
            {
                return this.Distance * Math.Cos(this.PivotAngle);
            }
        }

        public Vector3D LocalPosition
        {
            get
            {
                double num = Math.Sin(-this.PivotAngle) * this.Distance;
                return new Vector3D(num * Math.Sin(this.Rotation), -num * Math.Cos(this.Rotation), this.Distance * Math.Cos(this.PivotAngle));
            }
        }

        public CameraSnapshot()
        {
        }

        public CameraSnapshot(double latitude, double longitude, double rotation, double pivot, double distance)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Rotation = rotation;
            this.PivotAngle = pivot;
            this.Distance = distance;
        }

        public static CameraSnapshot FromUtcOffset(TimeSpan offsetFromUtc)
        {
            return new CameraSnapshot(0.0, offsetFromUtc.TotalHours * (Math.PI / 12.0), 0.0, 0.0, 1.8);
        }

        public static CameraSnapshot FromDegreesLatLong(double latitude, double longitude)
        {
            return new CameraSnapshot(latitude * (Math.PI / 180.0), longitude * (Math.PI / 180.0), 0.0, 0.0, 1.0);
        }

        public static CameraSnapshot Default()
        {
            return new CameraSnapshot(0.0, 0.0, 0.0, 0.0, 1.0);
        }

        public static CameraSnapshot DefaultForCustomMap()
        {
            return new CameraSnapshot(0.0, 0.0, 0.0, -0.61, 0.4);
        }

        public bool Equals(CameraSnapshot other)
        {
            if (other != null && this.Latitude == other.Latitude && (this.Longitude == other.Longitude && this.Rotation == other.Rotation) && this.PivotAngle == other.PivotAngle)
                return this.Distance == other.Distance;
            else
                return false;
        }

        public CameraSnapshot Clone()
        {
            return new CameraSnapshot(this.Latitude, this.Longitude, this.Rotation, this.PivotAngle, this.Distance);
        }
    }
}
