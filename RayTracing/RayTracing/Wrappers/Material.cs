using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing;

public abstract class Material : IDisposable
{
	internal MaterialSafeHandle Handle { get; }

	protected Material(MaterialSafeHandle handle)
	{
		Handle = handle;
	}

	public void Dispose()
	{
		Handle.Dispose(); 
		GC.SuppressFinalize(this);
	}
}

// Konkretne materiały
public sealed class Lambertian : Material
{
	public Lambertian(double r, double g, double b)
		: base(NativeMethods.CreateLambertian(r, g, b)) { }
}

public sealed class Metal : Material
{
	public Metal(double r, double g, double b, double fuzz)
		: base(NativeMethods.CreateMetal(r, g, b, fuzz)) { }
}

public sealed class Dielectric : Material
{
	public Dielectric(double refractionIndex)
		: base(NativeMethods.CreateDielectric(refractionIndex)) { }
}
