namespace RayTracing;

public struct Vec3
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vec3(double x, double y, double z)
    {
        X = x; Y = y; Z = z;
    }
}

public struct Color
{
    public double R { get; }
    public double G { get; }
    public double B { get; }

    public Color(double r, double g, double b)
    {
        R = r; G = g; B = b;
    }
}
