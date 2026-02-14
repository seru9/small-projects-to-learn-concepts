namespace RayTracing;

public sealed class Sphere : IDisposable
{
	internal SphereSafeHandle Handle { get; }

	// Obiektowe API: tworzymy sferę, podając obiekt Material, a nie surowy uchwyt!
	public Sphere(double centerX, double centerY, double centerZ, double radius, Material material)
	{
		ArgumentNullException.ThrowIfNull(material);
		Handle = NativeMethods.CreateSphere(centerX, centerY, centerZ, radius, material.Handle);
	}

	public void Dispose()
	{
		Handle.Dispose();
		GC.SuppressFinalize(this);
	}
}