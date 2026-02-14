using System;
using System.Runtime.InteropServices;

namespace RayTracing;

public sealed class MaterialSafeHandle : SafeHandle
{
	public MaterialSafeHandle() : base(nint.Zero, true) { }
	public override bool IsInvalid => handle == nint.Zero;

	protected override bool ReleaseHandle()
	{
		NativeMethods.DestroyMaterial(handle);
		return true;
	}
}

public sealed class SphereSafeHandle : SafeHandle
{
	public SphereSafeHandle() : base(nint.Zero, true) { }

	public override bool IsInvalid => handle == nint.Zero;

	protected override bool ReleaseHandle()
	{
		NativeMethods.DestroySphere(handle);
		return true;
	}
}

public sealed class SceneSafeHandle : SafeHandle
{
	public SceneSafeHandle() : base(nint.Zero, true) { }

	public override bool IsInvalid => handle == nint.Zero;

	protected override bool ReleaseHandle()
	{
		NativeMethods.DestroyScene(handle);
		return true;
	}
}
