namespace RayTracing;

public sealed class Scene : IDisposable
{
	internal SceneSafeHandle Handle { get; }

	public Scene()
	{
		Handle = NativeMethods.CreateScene();
	}

	// Zamiast NativeMethods.SceneAddSphere(scene, sphere), mamy idiomatyczne scene.Add(sphere)
	public void Add(Sphere sphere)
	{
		ArgumentNullException.ThrowIfNull(sphere);
		NativeMethods.SceneAddSphere(Handle, sphere.Handle);
	}

	public void Dispose()
	{
		Handle.Dispose();
		GC.SuppressFinalize(this);
	}
}