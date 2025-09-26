        //FastSolver
        #region FastSolver
        /*=======================================================================================                             
        Function: FastSolver                    
        ---------------------------------------                            
        function will: Efficiently solve equations
        //----------==--------=------------=-----------=---------------=------------=-----=-----*/
        using System;

        namespace IngameScript
        {
            public class FastSolver
            {
                public static readonly double epsilon = 0.000001;

                public static readonly double cos120d = -0.5;
                public static readonly double sin120d = Math.Sin(Math.PI / 3.0);
                public static readonly double root3 = Math.Sqrt(3.0);

                public static readonly double inv3 = 1.0 / 3.0;
                public static readonly double inv9 = 1.0 / 9.0;
                public static readonly double inv54 = 1.0 / 54.0;

                //Shortcut Ignoring Of Complex Values And Return Smallest Real Number
                public static double Solve(double a, double b, double c, double d, double e)
                {
                    if (Math.Abs(a) < epsilon) a = (a >= 0 ? epsilon : -epsilon);
                    double inva = 1 / a;

                    b *= inva;
                    c *= inva;
                    d *= inva;
                    e *= inva;

                    double a3 = -c;
                    double b3 = b * d - 4 * e;
                    double c3 = -b * b * e - d * d + 4 * c * e;

                    double[] result;
                    bool chooseMaximal = SolveCubic(a3, b3, c3, out result);
                    double y = result[0];
                    if (chooseMaximal)
                    {
                        if (Math.Abs(result[1]) > Math.Abs(y)) y = result[1];
                        if (Math.Abs(result[2]) > Math.Abs(y)) y = result[2];
                    }

                    double q1, q2, p1, p2, squ;

                    double u = y * y - 4 * e;
                    if (Math.Abs(u) < epsilon)
                    {
                        q1 = q2 = y * 0.5;
                        u = b * b - 4 * (c - y);

                        if (Math.Abs(u) < epsilon)
                        {
                            p1 = p2 = b * 0.5;
                        }
                        else
                        {
                            squ = Math.Sqrt(u);
                            p1 = (b + squ) * 0.5;
                            p2 = (b - squ) * 0.5;
                        }
                    }
                    else
                    {
                        squ = Math.Sqrt(u);
                        q1 = (y + squ) * 0.5;
                        q2 = (y - squ) * 0.5;

                        double dm = 1 / (q1 - q2);
                        p1 = (b * q1 - d) * dm;
                        p2 = (d - b * q2) * dm;
                    }

                    double v1, v2;

                    u = p1 * p1 - 4 * q1;
                    if (u < 0)
                    {
                        v1 = double.MaxValue;
                    }
                    else
                    {
                        squ = Math.Sqrt(u);
                        v1 = MinPosNZ(-p1 + squ, -p1 - squ) * 0.5;
                    }

                    u = p2 * p2 - 4 * q2;
                    if (u < 0)
                    {
                        v2 = double.MaxValue;
                    }
                    else
                    {
                        squ = Math.Sqrt(u);
                        v2 = MinPosNZ(-p2 + squ, -p2 - squ) * 0.5;
                    }

                    return MinPosNZ(v1, v2);
                }

                private static bool SolveCubic(double a, double b, double c, out double[] result)
                {
                    result = new double[4];

                    double a2 = a * a;
                    double q = (a2 - 3 * b) * inv9;
                    double r = (a * (2 * a2 - 9 * b) + 27 * c) * inv54;
                    double r2 = r * r;
                    double q3 = q * q * q;

                    if (r2 < q3)
                    {
                        double sqq = Math.Sqrt(q);

                        double t = r / (sqq * sqq * sqq);
                        if (t < -1) t = -1;
                        else if (t > 1) t = 1;

                        t = Math.Acos(t);

                        a *= inv3;
                        q = -2 * sqq;

                        double costv3 = Math.Cos(t * inv3);
                        double sintv3 = Math.Sin(t * inv3);

                        result[0] = q * costv3 - a;
                        result[1] = q * ((costv3 * cos120d) - (sintv3 * sin120d)) - a;
                        result[2] = q * ((costv3 * cos120d) + (sintv3 * sin120d)) - a;

                        return true;
                    }
                    else
                    {
                        double g = -Math.Pow(Math.Abs(r) + Math.Sqrt(r2 - q3), inv3);
                        if (r < 0) g = -g;

                        double h = (g == 0 ? 0 : q / g);

                        a *= inv3;

                        result[0] = (g + h) - a;
                        result[1] = -0.5 * (g + h) - a;
                        result[2] = 0.5 * root3 * (g - h);

                        if (Math.Abs(result[2]) < epsilon)
                        {
                            result[2] = result[1];
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                private static double MinPosNZ(double a, double b)
                {
                    if (a <= 0) return (b > 0 ? b : double.MaxValue);
                    else if (b <= 0) return a;
                    else return Math.Min(a, b);
                }
            }
        }
        #endregion