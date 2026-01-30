namespace RayTracing;

public abstract class Material : IDisposable
{
    internal MaterialSafeHandle Handle { get; }
    private bool _disposed;

    internal Material(MaterialSafeHandle handle)
    {
        Handle = handle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Handle.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~Material()
    {
        Dispose();
    }

    public static Material Lambertian(Color albedo) =>
        new LambertianMaterial(NativeMethods.CreateLambertian(albedo.R, albedo.G, albedo.B));

    public static Material Lambertian(double r, double g, double b) =>
        new LambertianMaterial(NativeMethods.CreateLambertian(r, g, b));

    public static Material Metal(Color albedo, double fuzz) =>
        new MetalMaterial(NativeMethods.CreateMetal(albedo.R, albedo.G, albedo.B, fuzz));

    public static Material Metal(double r, double g, double b, double fuzz) =>
        new MetalMaterial(NativeMethods.CreateMetal(r, g, b, fuzz));

    public static Material Dielectric(double refractionIndex) =>
        new DielectricMaterial(NativeMethods.CreateDielectric(refractionIndex));
}

internal class LambertianMaterial : Material
{
    internal LambertianMaterial(MaterialSafeHandle handle) : base(handle) { }
}

internal class MetalMaterial : Material
{
    internal MetalMaterial(MaterialSafeHandle handle) : base(handle) { }
}

internal class DielectricMaterial : Material
{
    internal DielectricMaterial(MaterialSafeHandle handle) : base(handle) { }
}
