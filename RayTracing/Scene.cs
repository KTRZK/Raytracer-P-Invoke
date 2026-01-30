namespace RayTracing;

public class Scene : IDisposable
{
    internal SceneSafeHandle Handle { get; }
    private readonly List<Material> _materials = new();
    private bool _disposed;

    public Scene()
    {
        Handle = NativeMethods.CreateScene();
    }

    public void AddSphere(Vec3 center, double radius, Material material)
    {
        _materials.Add(material);
        NativeMethods.SceneAddSphere(Handle, center.X, center.Y, center.Z, radius, material.Handle);
    }

    public void AddSphere(double cx, double cy, double cz, double radius, Material material)
    {
        _materials.Add(material);
        NativeMethods.SceneAddSphere(Handle, cx, cy, cz, radius, material.Handle);
    }

    public void Clear()
    {
        NativeMethods.SceneClear(Handle);
        foreach (var mat in _materials)
            mat.Dispose();
        _materials.Clear();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Handle.Dispose();
            foreach (var mat in _materials)
                mat.Dispose();
            _materials.Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~Scene()
    {
        Dispose();
    }
}
