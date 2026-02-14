using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace RayTracing;
[StructLayout(LayoutKind.Sequential)]
public struct CameraConfig
{
	public double aspect_ratio;
	public int image_width;
	public int samples_per_pixel;
	public int max_depth;

	public double vfov;
	public double lookfromX, lookfromY, lookfromZ;
	public double lookatX, lookatY, lookatZ;
	public double vupX, vupY, vupZ;

	public double defocus_angle;
	public double focus_dist;
}
public delegate void RenderCallback(int samples, nint buffer); // Jak powinno wygl¹daæ
public static partial class NativeMethods
{
	private const string LibName = "rt";


	[LibraryImport(LibName, EntryPoint = "CreateMaterial")] //EntryPint to nazwa funkcji w bibliotece natywnej
	public static partial MaterialSafeHandle CreateMaterial();

	[LibraryImport(LibName, EntryPoint = "DestroyMaterial")]
	public static partial void DestroyMaterial(nint materialHandle);

	[LibraryImport(LibName, EntryPoint = "CreateLambertian")] 
	public static partial MaterialSafeHandle CreateLambertian(double r, double g, double b);

	[LibraryImport(LibName, EntryPoint = "CreateMetal")] 
	public static partial MaterialSafeHandle CreateMetal(double r, double g, double b, double fuzz);


	[LibraryImport(LibName, EntryPoint = "CreateDielectric")] 
	public static partial MaterialSafeHandle CreateDielectric(double refractionIndex);

	[LibraryImport(LibName, EntryPoint = "CreateSphere")]
	public static partial SphereSafeHandle CreateSphere(double centerX, double centerY, double centerZ, double radius, MaterialSafeHandle materialPtr);

	[LibraryImport(LibName, EntryPoint = "DestroySphere")]
	public static partial void DestroySphere(nint sphereHandle);
	[LibraryImport(LibName, EntryPoint = "CreateScene")]
	public static partial SceneSafeHandle CreateScene();
	[LibraryImport(LibName, EntryPoint = "DestroyScene")]
	public static partial void DestroyScene(nint sceneHandle);

	[LibraryImport(LibName, EntryPoint = "SceneAddSphere")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool SceneAddSphere(SceneSafeHandle sceneHandle, SphereSafeHandle sphereHandle);

	[LibraryImport(LibName, EntryPoint = "RenderScene")]
	public static partial void RenderScene(CameraConfig cfg, SceneSafeHandle sceneHandle, nint rgbaBuffer, RenderCallback renderCallback);

	[LibraryImport(LibName, EntryPoint = "SavePng", StringMarshalling = StringMarshalling.Utf8)] //const* char wiêc UTF-8
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool SavePng(string filePath, int width, int height, nint rgbaBuffer);


}