using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RayTracing;

class Program
{
	// Bufor RGBA8
	private static byte[]? _rgba;
	private static GCHandle _rgbaHandle;
	private static nint _rgbaPtr;

	// Postêp próbek
	private static volatile int _lastSamples;

	static void Main()
	{
		// Konfiguracja kamery (RTIOW: ok³adka)
		var cfg = new CameraConfig
		{
			aspect_ratio = 3.0 / 2.0,
			image_width = 1200,
			samples_per_pixel = 500,
			max_depth = 50,
			vfov = 20.0,
			lookfromX = 13,
			lookfromY = 2,
			lookfromZ = 3,
			lookatX = 0,
			lookatY = 0,
			lookatZ = 0,
			vupX = 0,
			vupY = 1,
			vupZ = 0,
			defocus_angle = 0.6,
			focus_dist = 10.0
		};

		int width = cfg.image_width;
		int height = (int)(width / cfg.aspect_ratio);

		// Alokacja bufora obrazu
		_rgba = new byte[width * height * 4];
		_rgbaHandle = GCHandle.Alloc(_rgba, GCHandleType.Pinned);
		_rgbaPtr = _rgbaHandle.AddrOfPinnedObject();

		// Utworzenie sceny
		var scene = NativeMethods.CreateScene();

		// Proceduralna scena:
		// Materia³y
		var groundMat = NativeMethods.CreateLambertian(0.5, 0.5, 0.5);
		var centerMat = NativeMethods.CreateLambertian(0.1, 0.2, 0.5);
		var leftMat = NativeMethods.CreateDielectric(1.5);
		var rightMat = NativeMethods.CreateMetal(0.8, 0.6, 0.2, 0.0);

		// Sfery (ok³adka RTIOW)
		var ground = NativeMethods.CreateSphere(0.0, -1000.0, 0.0, 1000.0, groundMat);
		var center = NativeMethods.CreateSphere(0.0, 1.0, 0.0, 1.0, centerMat);
		var left = NativeMethods.CreateSphere(-4.0, 1.0, 0.0, 1.0, leftMat);
		var right = NativeMethods.CreateSphere(4.0, 1.0, 0.0, 1.0, rightMat);

		NativeMethods.SceneAddSphere(scene, ground);
		NativeMethods.SceneAddSphere(scene, center);
		NativeMethods.SceneAddSphere(scene, left);
		NativeMethods.SceneAddSphere(scene, right);

		// Ma³e kule na ziemi (losowo)
		var rnd = new Random(42);
		for (int a = -11; a < 11; a++)
		{
			for (int b = -11; b < 11; b++)
			{
				double chooseMat = rnd.NextDouble();
				var centerX = a + 0.9 * rnd.NextDouble();
				var centerZ = b + 0.9 * rnd.NextDouble();
				var centerY = 0.2;

				// Oddal je od du¿ych kul
				var dx = centerX - 4.0;
				var dz = centerZ - 0.0;
				if (Math.Sqrt(dx * dx + dz * dz) <= 0.9) continue;

				// Zamieñ typ zmiennej mat z nint na MaterialSafeHandle
				MaterialSafeHandle mat;
				if (chooseMat < 0.8)
				{
					// Lambertian
					var r = rnd.NextDouble() * rnd.NextDouble();
					var g = rnd.NextDouble() * rnd.NextDouble();
					var bcol = rnd.NextDouble() * rnd.NextDouble();
					mat = NativeMethods.CreateLambertian(r, g, bcol);
				}
				else if (chooseMat < 0.95)
				{
					// Metal
					var r = 0.5 * (1 + rnd.NextDouble());
					var g = 0.5 * (1 + rnd.NextDouble());
					var bcol = 0.5 * (1 + rnd.NextDouble());
					var fuzz = rnd.NextDouble() * 0.5;
					mat = NativeMethods.CreateMetal(r, g, bcol, fuzz);
				}
				else
				{
					// Dielectric
					mat = NativeMethods.CreateDielectric(1.5);
				}

				var small = NativeMethods.CreateSphere(centerX, centerY, centerZ, 0.2, mat);
				NativeMethods.SceneAddSphere(scene, small);
			}
		}

		// Miernik czasu
		var sw = Stopwatch.StartNew();

		// Start okna i pêtli renderowania
		Windowing.Viewer.Show(width, height, "Ray Tracing in One Weekend - Demo", updater =>
		{
			// Callback renderuj¹cy: dostaje liczbê próbek oraz wskaŸnik na bufor
			RenderCallback progress = (samples, buffer) =>
			{
				_lastSamples = samples;
				// Odœwie¿ wizualizacjê
				updater.UpdateImage(new ReadOnlySpan<byte>(_rgba!)); // zaktualizuj obraz
				updater.UpdateStatus($"Samples: {samples}  Time: {sw.Elapsed:mm\\:ss}");
			};

			// Wywo³anie natywnego renderu (renderuje progresywnie i wo³a callback)
			NativeMethods.RenderScene(cfg, scene, _rgbaPtr, progress);

			// Zapis finalnego obrazu
			var ok = NativeMethods.SavePng("output.png", width, height, _rgbaPtr);
			Console.WriteLine(ok ? "Zapisano output.png" : "B³¹d zapisu PNG");

			_rgbaHandle.Free();
		});
	}
}