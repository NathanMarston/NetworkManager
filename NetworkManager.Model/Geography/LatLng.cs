using System;

namespace NetworkManager.Model.Geography
{
    public class LatLng
    {
        public float Latitude;
        public float Longitude;

        public LatLng(float lat, float lng)
        {
            this.Latitude = lat;
            this.Longitude = lng;
        }

        /// <summary>
        /// Converts from Lambert Conformal Conic (LCC) to LatLong coords
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public LatLng(long x, long y)
        {
            double ee = Convert.ToInt32(x);
            double nn = Convert.ToInt32(y);

            const double pi = 3.14159265358979;
            const double a = 6378137; //equatorial radius
            const double b = 6356752.314; // polar radius
            const double one = 1;
            const double two = 2;
            double e = Math.Pow(one - Math.Pow((b / a), two), 0.5);//'0.081819191
            double e2 = e * e / (1 - e * e);//'0.006739497
            double phi1 = -28 * pi / 180;
            double phi2 = -36 * pi / 180;
            const double phio = -32 * pi / 180;
            const double lamdao = 135 * pi / 180;


            const double ef = 1000000;
            const double nf = 2000000;

            ee = ee / 100;
            nn = nn / 100;

            double m1 = Math.Cos(phi1) / Math.Sqrt(1 - e2 * Math.Sin(phi1) * Math.Sin(phi1));
            double m2 = Math.Cos(phi2) / Math.Sqrt(1 - e2 * Math.Sin(phi2) * Math.Sin(phi2));

            double t1 = Math.Tan((pi / 4) - (phi1 / 2)) / Math.Pow(((1 - e * Math.Sin(phi1)) / (1 + e * Math.Sin(phi1))), (e / 2));
            double t2 = Math.Tan((pi / 4) - (phi2 / 2)) / Math.Pow(((1 - e * Math.Sin(phi2)) / (1 + e * Math.Sin(phi2))), (e / 2));
            double t0 = Math.Tan((pi / 4) - (phio / 2)) / Math.Pow(((1 - e * Math.Sin(phio)) / (1 + e * Math.Sin(phio))), (e / 2));
            double n = (Math.Log(m1) - Math.Log(m2)) / (Math.Log(t1) - Math.Log(t2));

            double ff = m1 / (n * Math.Pow(t1, n));
            double rf = a * ff * Math.Pow(t0, n);
            double r = Math.Sqrt(Math.Pow((ee - ef), 2) + Math.Pow((rf - (nn - nf)), 2));

            double t;
            if (Math.Sign(r / (a * ff)) == -1)
            {
                t = Math.Pow(Math.Abs((r / (a * ff))), (1 / n));
                t = t * -1;
            }
            else
            {
                t = Math.Pow((r / (a * ff)), (1 / n));
            }

            double theta = Math.Atan((ee - ef) / (rf - (nn - nf)));
            double lamda = theta / n + lamdao;
            double phi0 = (pi / 2) - (2 * Math.Atan(t));


            phi1 = (pi / 2) - 2 * Math.Atan(t * Math.Pow(((1 - e * Math.Sin(phi0)) / (1 + e * Math.Sin(phi0))), (e / 2)));
            phi2 = (pi / 2) - 2 * Math.Atan(t * Math.Pow(((1 - e * Math.Sin(phi1)) / (1 + e * Math.Sin(phi1))), (e / 2)));
            double phi = (pi / 2) - 2 * Math.Atan(t * Math.Pow(((1 - e * Math.Sin(phi2)) / (1 + e * Math.Sin(phi2))), (e / 2)));


            this.Latitude = (float)(180 - phi * 180 / pi);
            this.Longitude = (float)(lamda * 180 / pi);
        }

        protected bool Equals(LatLng other)
        {
            return Latitude == other.Latitude && Longitude == other.Longitude;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((LatLng)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }
    }
}

