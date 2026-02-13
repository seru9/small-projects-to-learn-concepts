using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace RayTracing;

internal static partial class NativeMethods
{
    private const string LibName = "rt";
	[LibraryImport(LibName, EntryPoint = "CreateMaterial")] //EntryPint to nazwa funkcji w bibliotece natywnej
	public static partial nint CreateMaterial();

	[LibraryImport(LibName, EntryPoint = "DestroyMaterial")]
	public static partial void DestroyMaterial(nint materialHandle);

	[LibraryImport(LibName, EntryPoint = "CreateSphere")]
	public static partial nint CreateSphere();

	[LibraryImport(LibName, EntryPoint = "DestroySphere")]
	public static partial void DestroySphere(nint sphereHandle);
	[LibraryImport(LibName, EntryPoint = "CreateScene")]
	public static partial nint CreateScene();
	[LibraryImport(LibName, EntryPoint = "DestroyScene")]
	public static partial void DestroyScene(nint sceneHandle);
	[LibraryImport(LibName, EntryPoint = "RenderScene")]
	public static partial void RenderScene(nint sceneHandle, nint outputImageHandle);

}
public class StringSafeHandle : SafeHandle
{
	public StringSafeHandle() : base(nint.Zero, true) { }

	protected override bool ReleaseHandle()
	{
		//NativeMethods.DestroyString(handle);
		handle = nint.Zero;
		return true;
	}

	public override bool IsInvalid => handle == nint.Zero;
}