using System.Runtime.InteropServices;

namespace RayTracing;

public static class RenderClass
{
	public static void Render(CameraConfig config, Scene scene, nint rgbaBuffer, RenderCallback renderCallback)
	{
		ArgumentNullException.ThrowIfNull(scene);
		NativeMethods.RenderScene(config, scene.Handle, rgbaBuffer, renderCallback);
	}
}