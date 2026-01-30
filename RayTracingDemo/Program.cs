using RayTracing;
using Windowing;

namespace RayTracingDemo;

class Program
{
    static void Main(string[] args)
    {
        var settings = new CameraSettings
        {
            AspectRatio = 16.0 / 9.0,
            ImageWidth = 400,
            SamplesPerPixel = 50,
            MaxDepth = 30,
            Vfov = 20,
            LookFrom = new Vec3(13, 2, 3),
            LookAt = new Vec3(0, 0, 0),
            Vup = new Vec3(0, 1, 0),
            DefocusAngle = 0.6,
            FocusDist = 10.0
        };

        int width = settings.ImageWidth;
        int height = settings.ImageHeight;

        Console.WriteLine($"Ray Tracing Demo - {width}x{height}, {settings.SamplesPerPixel} samples");

        byte[]? finalBuffer = null;

        Viewer.Show(width, height, "Ray Tracing in One Weekend", (window) =>
        {
            try
            {
                Console.WriteLine("Building scene...");
                using var scene = BuildScene();
                Console.WriteLine("Scene built");

                int bufferSize = width * height * 4;
                byte[] buffer = new byte[bufferSize];

                var startTime = DateTime.Now;
                Console.WriteLine("Starting render...");

                Renderer.RenderToBuffer(scene, settings, buffer, (current, total, data) =>
                {
                    if (window.IsClosed) return;

                    double elapsed = (DateTime.Now - startTime).TotalSeconds;
                    double eta = current > 0 ? (elapsed / current) * (total - current) : 0;

                    string status = $"Sample {current}/{total} | Elapsed: {elapsed:F1}s | ETA: {eta:F1}s";
                    Console.WriteLine(status);
                    window.UpdateStatus(status);
                    window.UpdateImage(data);
                });

                finalBuffer = buffer;
                double totalTime = (DateTime.Now - startTime).TotalSeconds;

                if (!window.IsClosed)
                {
                    string finalStatus = $"Done! Total time: {totalTime:F1}s";
                    Console.WriteLine(finalStatus);
                    window.UpdateStatus(finalStatus);

                    Renderer.SavePng("output.png", width, height, buffer);
                    Console.WriteLine("Saved: output.png");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
            }
        });
    }    static Scene BuildScene()
    {
        var scene = new Scene();
        var rng = new Random(42);

        var groundMaterial = Material.Lambertian(0.5, 0.5, 0.5);
        scene.AddSphere(0, -1000, 0, 1000, groundMaterial);

        for (int a = -11; a < 11; a++)
        {
            for (int b = -11; b < 11; b++)
            {
                double chooseMat = rng.NextDouble();
                double cx = a + 0.9 * rng.NextDouble();
                double cz = b + 0.9 * rng.NextDouble();

                double dx = cx - 4;
                double dz = cz;
                if (Math.Sqrt(dx * dx + 0.04 + dz * dz) > 0.9)
                {
                    Material mat;
                    if (chooseMat < 0.8)
                    {
                        double r = rng.NextDouble() * rng.NextDouble();
                        double g = rng.NextDouble() * rng.NextDouble();
                        double bl = rng.NextDouble() * rng.NextDouble();
                        mat = Material.Lambertian(r, g, bl);
                    }
                    else if (chooseMat < 0.95)
                    {
                        double r = 0.5 + 0.5 * rng.NextDouble();
                        double g = 0.5 + 0.5 * rng.NextDouble();
                        double bl = 0.5 + 0.5 * rng.NextDouble();
                        double fuzz = 0.5 * rng.NextDouble();
                        mat = Material.Metal(r, g, bl, fuzz);
                    }
                    else
                    {
                        mat = Material.Dielectric(1.5);
                    }
                    scene.AddSphere(cx, 0.2, cz, 0.2, mat);
                }
            }
        }

        var mat1 = Material.Dielectric(1.5);
        scene.AddSphere(0, 1, 0, 1.0, mat1);

        var mat2 = Material.Lambertian(0.4, 0.2, 0.1);
        scene.AddSphere(-4, 1, 0, 1.0, mat2);

        var mat3 = Material.Metal(0.7, 0.6, 0.5, 0.0);
        scene.AddSphere(4, 1, 0, 1.0, mat3);

        return scene;
    }
}
