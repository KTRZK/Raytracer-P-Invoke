using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace RayTracing;

[StructLayout(LayoutKind.Sequential)]
internal struct CameraConfig
{
    public double AspectRatio;
    public int ImageWidth;
    public int SamplesPerPixel;
    public int MaxDepth;
    public double Vfov;
    public double LookFromX, LookFromY, LookFromZ;
    public double LookAtX, LookAtY, LookAtZ;
    public double VupX, VupY, VupZ;
    public double DefocusAngle;
    public double FocusDist;
}

internal delegate void RenderCallbackDelegate(int samples, IntPtr buffer);

internal class SceneSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SceneSafeHandle() : base(true) { }
    
    protected override bool ReleaseHandle()
    {
        NativeMethods.DestroyScene(handle);
        return true;
    }
}

internal class MaterialSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public MaterialSafeHandle() : base(true) { }
    
    protected override bool ReleaseHandle()
    {
        NativeMethods.DestroyMaterial(handle);
        return true;
    }
}

internal static partial class NativeMethods
{
    private const string LibName = "rt";

    [LibraryImport(LibName, EntryPoint = "CreateScene")]
    public static partial SceneSafeHandle CreateScene();

    [LibraryImport(LibName, EntryPoint = "DestroyScene")]
    public static partial void DestroyScene(IntPtr scene);

    [LibraryImport(LibName, EntryPoint = "SceneClear")]
    public static partial void SceneClear(SceneSafeHandle scene);

    [LibraryImport(LibName, EntryPoint = "SceneAddSphere")]
    public static partial void SceneAddSphere(SceneSafeHandle scene, double cx, double cy, double cz, double radius, MaterialSafeHandle material);

    [LibraryImport(LibName, EntryPoint = "CreateLambertian")]
    public static partial MaterialSafeHandle CreateLambertian(double r, double g, double b);

    [LibraryImport(LibName, EntryPoint = "CreateMetal")]
    public static partial MaterialSafeHandle CreateMetal(double r, double g, double b, double fuzz);

    [LibraryImport(LibName, EntryPoint = "CreateDielectric")]
    public static partial MaterialSafeHandle CreateDielectric(double refractionIndex);

    [LibraryImport(LibName, EntryPoint = "DestroyMaterial")]
    public static partial void DestroyMaterial(IntPtr material);

    [LibraryImport(LibName, EntryPoint = "RenderScene")]
    public static partial void RenderScene(SceneSafeHandle scene, CameraConfig config, IntPtr buffer, RenderCallbackDelegate callback);

    [LibraryImport(LibName, EntryPoint = "GetImageHeight")]
    public static partial int GetImageHeight(int width, double aspectRatio);

    [LibraryImport(LibName, EntryPoint = "SavePng", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int SavePng(string filename, int width, int height, IntPtr data);
}
