using Microsoft.Data.Visualization.Engine.MathExtensions;
using System;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    public struct Coordinates
    {
        public const double MaxMercatorLat = 1.48442225104172;
        private const double MaxSinMercatorLat = 0.99627207805792;
        public const double MaxFlatMapY = 3.14159290045661;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double LatitudeInDegrees
        {
            get
            {
                return this.Latitude * Constants.DegreesPerRadian;
            }
        }

        public double LongitudeInDegrees
        {
            get
            {
                return this.Longitude * Constants.DegreesPerRadian;
            }
        }

        public Vector3D Position
        {
            get
            {
                return Coordinates.GeoTo3D(this.Longitude, this.Latitude);
            }
        }

        public Coordinates(double lon, double lat)
        {
            this = new Coordinates();
            this.Latitude = lat;
            this.Longitude = lon;
        }

        public static bool operator ==(Coordinates left, Coordinates right)
        {
            return left.Latitude == right.Latitude && left.Longitude == right.Longitude;
        }

        public static bool operator !=(Coordinates left, Coordinates right)
        {
            return left.Latitude != right.Latitude && left.Longitude != right.Longitude;
        }

        public static Coordinates FromDegrees(double lon, double lat)
        {
            return new Coordinates(lon * (Math.PI / 180.0), lat * (Math.PI / 180.0));
        }

        public static Vector3D GeoTo3D(double lon, double lat)
        {
            double num = Math.Cos(lat);
            return new Vector3D(Math.Cos(lon) * num, Math.Sin(lat), Math.Sin(lon) * num);
        }

        public static Vector3D GeoTo3DFlattening(double lon, double lat, double flattening)
        {
            if (flattening == 0.0)
                return GeoTo3D(lon, lat);
            Vector3D position;
            Vector3D normal;
            ComputeWarp(lat, lon, flattening, out position, out normal);
            return position;
        }

        public Vector3D To3D(bool flatMap)
        {
            if (flatMap)
                return new Vector3D(1.0, Coordinates.Mercator(this.Latitude), this.Longitude);
            else
                return Coordinates.GeoTo3D(this.Longitude, this.Latitude);
        }

        public static Coordinates World3DToGeo(Vector3D worldPos)
        {
            double num = Math.Sqrt(Vector3D.Dot(worldPos, worldPos));
            return new Coordinates(Math.Atan2(worldPos.Z, worldPos.X), Math.Asin(worldPos.Y / num));
        }

        public static Coordinates Flat3DToGeo(Vector3D worldPos)
        {
            return new Coordinates(worldPos.Z, Coordinates.InverseMercator(worldPos.Y));
        }

        public static Vector2F Vector3FToGeo(Vector3F vector)
        {
            vector.AssertIsUnitVector();
            return new Vector2F((float)Math.Atan2(vector.Z, vector.X), (float)Math.Asin(vector.Y));
        }

        public static Coordinates World3DToGeo(Vector3D worldPos, bool flatMap)
        {
            if (flatMap)
                return Coordinates.Flat3DToGeo(worldPos);
            else
                return Coordinates.World3DToGeo(worldPos);
        }

        public static Vector3D GetNorthVector(Vector3D point)
        {
            point.AssertIsUnitVector();
            Vector3D vector3D = Vector3D.YVector;
            if (vector3D.Orthonormalize(point))
                return vector3D;
            else
                return Vector3D.XVector;
        }

        /// <summary>
        /// 计算目标点向量的局部坐标系的三个轴的指向
        /// </summary>
        /// <param name="center">目标点向量，为局部坐标系的Up轴，对应全局坐标系X轴</param>
        /// <param name="north">计算得到north向量，为局部坐标系的North轴，指向北极，对应全局坐标系Y轴</param>
        /// <param name="east">计算得到east向量，为局部坐标系的East轴，对应全局坐标系Z轴，与North轴/Up轴满足左手系规则</param>
        public static void GetLocalFrame(Vector3D center, out Vector3D north, out Vector3D east)
        {
            north = Coordinates.GetNorthVector(center);
            east = Vector3D.Cross(center, north);
        }

        public static double GetAzimuth(Vector3D position, Vector3D direction)
        {
            position.AssertIsUnitVector();
            direction.AssertIsUnitVector();
            if (MathEx.Square(position.X) + MathEx.Square(position.Z) < Constants.Fuzz)
            {
                return position.Y < 0.0 ? 0.0 : Math.PI;
            }
            else
            {
                Vector3D north;
                Vector3D east;
                Coordinates.GetLocalFrame(position, out north, out east);
                return Math.Atan2(direction * east, direction * north);
            }
        }

        public static Vector3D GetDirectionVector(Vector3D position, double azimuth)
        {
            position.AssertIsUnitVector();
            Vector3D north;
            Vector3D east;
            Coordinates.GetLocalFrame(position, out north, out east);
            return north * Math.Cos(azimuth) + east * Math.Sin(azimuth);
        }

        public static Vector3D GetDirectionVector(bool flatMap, Vector3D position, double azimuth)
        {
            if (!flatMap)
                return Coordinates.GetDirectionVector(position, azimuth);
            else
                return new Vector3D(0.0, Math.Cos(azimuth), Math.Sin(azimuth)) / Constants.SqrtOf2;
        }

        public static double Mercator(double latitude)
        {
            return Math.Log(Math.Tan(Math.PI / 4.0 + Math.Max(-MaxMercatorLat, Math.Min(MaxMercatorLat, latitude)) / 2.0));
        }

        public static double MercatorFromSine(double sine)
        {
            if (sine > MaxSinMercatorLat)
                sine = MaxSinMercatorLat;
            else if (sine < -MaxSinMercatorLat)
                sine = -MaxSinMercatorLat;
            double actual = Math.Log((1.0 + sine) / (1.0 - sine)) / 2.0;
            MathEx.AssertEqual(actual, Coordinates.Mercator(Math.Asin(sine)), 0.001);
            return actual;
        }

        public static double InverseMercator(double y)
        {
            return 2.0 * Math.Atan(Math.Exp(y)) - Math.PI / 2.0;
        }

        public static DifferentiableScalar InverseMercator(DifferentiableScalar y)
        {
            return new DifferentiableScalar(2.0 * Math.Atan(Math.Exp(y.Value)) - Math.PI / 2.0, y.Derivative / Math.Cosh(y.Value));
        }

        public static void GetPointOnArc(double radius, double cos, double sin, double xMid, out double x, out double y)
        {
            double num = cos > 0.0 ? sin * sin / (1.0 + cos) : 1.0 - cos;
            x = xMid - radius * num;
            y = radius * sin;
        }

        public static void ComputeWarp(double lat, double lon, double flattening, out Vector3D position, out Vector3D normal)
        {
            if (flattening <= 0.0)
            {
                position = Coordinates.GeoTo3D(lon, lat);
                normal = position;
            }
            else
            {
                double num1 = 1.0 - flattening;
                if (num1 <= Constants.Fuzz)
                {
                    position = new Vector3D(1.0, Coordinates.Mercator(lat), lon);
                    normal = Vector3D.XVector;
                }
                else
                {
                    double radius = 1.0 / num1;
                    double num2 = num1 * Coordinates.Mercator(lat);
                    double num3 = Math.Tanh(num2);
                    double cos1 = 1.0 / Math.Cosh(num2);
                    double x1;
                    double y1;
                    Coordinates.GetPointOnArc(radius, cos1, num3, 1.0, out x1, out y1);
                    double num4 = num1 * lon;
                    double cos2 = Math.Cos(num4);
                    double sin = Math.Sin(num4);
                    double x2;
                    double y2;
                    Coordinates.GetPointOnArc(radius * cos1, cos2, sin, x1, out x2, out y2);
                    position = new Vector3D(x2, y1, y2);
                    normal = new Vector3D(cos2 * cos1, num3, sin * cos1);
                    normal.AssertIsUnitVector();
                }
            }
        }

        public static Vector3D UnitSphereToFlatMap(Vector3D position)
        {
            position.AssertIsUnitVector();
            return new Vector3D(1.0, Coordinates.MercatorFromSine(position.Y), Math.Atan2(position.Z, position.X));
        }

        public static Vector3D ComputePosition(Vector3D sphericalPosition, bool flatMap)
        {
            sphericalPosition.AssertIsUnitVector();
            if (flatMap)
                return Coordinates.UnitSphereToFlatMap(sphericalPosition);
            else
                return sphericalPosition;
        }

        public bool IsOutOfTheWorld(bool flatMap)
        {
            if (!flatMap)
                return Math.Abs(this.Latitude) > Math.PI;
            if (Math.Abs(this.Longitude) <= Math.PI)
                return Math.Abs(this.Latitude) > MaxMercatorLat;
            else
                return true;
        }
    }
}
