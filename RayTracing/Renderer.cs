using System.Runtime.InteropServices;

namespace RayTracing;

public class CameraSettings
{
    public double AspectRatio { get; set; } = 16.0 / 9.0;
    public int ImageWidth { get; set; } = 400;
    public int SamplesPerPixel { get; set; } = 100;
    public int MaxDepth { get; set; } = 50;
    public double Vfov { get; set; } = 20;
    public Vec3 LookFrom { get; set; } = new(13, 2, 3);
    public Vec3 LookAt { get; set; } = new(0, 0, 0);
    public Vec3 Vup { get; set; } = new(0, 1, 0);
    public double DefocusAngle { get; set; } = 0.6;
    public double FocusDist { get; set; } = 10.0;

    public int ImageHeight => NativeMethods.GetImageHeight(ImageWidth, AspectRatio);

    internal CameraConfig ToNative() => new CameraConfig
    {
        AspectRatio = AspectRatio,
        ImageWidth = ImageWidth,
        SamplesPerPixel = SamplesPerPixel,
        MaxDepth = MaxDepth,
        Vfov = Vfov,
        LookFromX = LookFrom.X,
        LookFromY = LookFrom.Y,
        LookFromZ = LookFrom.Z,
        LookAtX = LookAt.X,
        LookAtY = LookAt.Y,
        LookAtZ = LookAt.Z,
        VupX = Vup.X,
        VupY = Vup.Y,
        VupZ = Vup.Z,
        DefocusAngle = DefocusAngle,
        FocusDist = FocusDist
    };
}

public delegate void RenderProgressCallback(int currentSample, int totalSamples, ReadOnlySpan<byte> buffer);

public static class Renderer
{
    public static void Render(Scene scene, CameraSettings settings, RenderProgressCallback? progressCallback = null)
    {
        int width = settings.ImageWidth;
        int height = settings.ImageHeight;
        int bufferSize = width * height * 4;
        
        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
        try
        {
            var config = settings.ToNative();
            
            RenderCallbackDelegate? nativeCallback = null;
            if (progressCallback != null)
            {
                nativeCallback = (samples, bufPtr) =>
                {
                    unsafe
                    {
                        var span = new ReadOnlySpan<byte>((void*)bufPtr, bufferSize);
                        progressCallback(samples, settings.SamplesPerPixel, span);
                    }
                };
            }
            
            NativeMethods.RenderScene(scene.Handle, config, buffer, nativeCallback!);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public static void RenderToBuffer(Scene scene, CameraSettings settings, byte[] outputBuffer, RenderProgressCallback? progressCallback = null)
    {
        int width = settings.ImageWidth;
        int height = settings.ImageHeight;
        int bufferSize = width * height * 4;
        
        if (outputBuffer.Length < bufferSize)
            throw new ArgumentException($"Buffer too small. Required: {bufferSize}, provided: {outputBuffer.Length}");

        GCHandle pinnedBuffer = GCHandle.Alloc(outputBuffer, GCHandleType.Pinned);
        try
        {
            IntPtr bufferPtr = pinnedBuffer.AddrOfPinnedObject();
            var config = settings.ToNative();
            
            RenderCallbackDelegate? nativeCallback = null;
            if (progressCallback != null)
            {
                nativeCallback = (samples, bufPtr) =>
                {
                    var span = new ReadOnlySpan<byte>(outputBuffer);
                    progressCallback(samples, settings.SamplesPerPixel, span);
                };
            }
            
            NativeMethods.RenderScene(scene.Handle, config, bufferPtr, nativeCallback!);
        }
        finally
        {
            pinnedBuffer.Free();
        }
    }

    public static void SavePng(string filename, int width, int height, byte[] data)
    {
        GCHandle pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            IntPtr dataPtr = pinnedData.AddrOfPinnedObject();
            int result = NativeMethods.SavePng(filename, width, height, dataPtr);
            if (result == 0)
                throw new InvalidOperationException("Failed to save PNG file");
        }
        finally
        {
            pinnedData.Free();
        }
    }
}
