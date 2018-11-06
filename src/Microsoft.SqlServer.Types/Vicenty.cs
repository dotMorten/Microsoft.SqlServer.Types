using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    internal class Vincenty
    {
        private const double D2R = 0.0174532925; //Degrees to radians

        /// <summary>
        /// Vincenty's formulae is used in geodesy to calculate the distance
        /// between two points on the surface of an spheroid.
        /// </summary>
        /// <param name="lat1">The start latitude.</param>
        /// <param name="lon1">The start longitude.</param>
        /// <param name="lat2">The end latitude.</param>
        /// <param name="lon2">The end longitude.</param>
        /// <param name="semiMajor">Meters</param>
        /// <param name="semiMinor">Meters</param>
        /// <returns>Distance in meters between the two points</returns>
        /// <remarks>
        /// Developed by Thaddeus Vincenty in 1975. They are based on the 
        /// assumption that the figure of the Earth is an oblate spheroid,
        /// and hence are more accurate than methods such as great-circle 
        /// distance which assume a spherical Earth.
        /// It is considered to be accurate to within 0.5 mm on the Earth Ellipsoid.
        /// </remarks>
        public static double GetDistanceVincenty(double lat1, double lon1, double lat2, double lon2,
            double semiMajor, double semiMinor)
        {
            var a = semiMajor;
            var b = semiMinor;
            var f = (a - b) / a; //flattening
            var L = (lon2 - lon1) * D2R;
            var U1 = Math.Atan((1 - f) * Math.Tan(lat1 * D2R));
            var U2 = Math.Atan((1 - f) * Math.Tan(lat2 * D2R));
            var sinU1 = Math.Sin(U1);
            var cosU1 = Math.Cos(U1);
            var sinU2 = Math.Sin(U2);
            var cosU2 = Math.Cos(U2);

            var lambda = L;
            double lambdaP;
            double cosSigma, cosSqAlpha, sinSigma, cos2SigmaM, sigma, sinLambda, cosLambda;

            int iterLimit = 100;
            do
            {
                sinLambda = Math.Sin(lambda);
                cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +
                    (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                if (sinSigma == 0)
                    return 0;  // co-incident points

                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cosSqAlpha = 1 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;
                if (double.IsNaN(cos2SigmaM))
                    cos2SigmaM = 0;  // equatorial line: cosSqAlpha=0 (§6)
                double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
                lambdaP = lambda;
                lambda = L + (1 - C) * f * sinAlpha *
                    (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

            if (iterLimit == 0) return double.NaN;  // formula failed to converge

            var uSq = cosSqAlpha * (a * a - b * b) / (b * b);
            var A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
            var B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));
            var deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
                B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
            var s = b * A * (sigma - deltaSigma);

            s = Math.Round(s, 3); // round to 1mm precision
            return s;
        }
    }
}
