using Sandbox;
using System;

namespace Heralds;

/// <summary>
/// "Behind them, danger rearranged itself." Heat is how close Aperture's
/// pale-backed hunters are. It rises slowly on its own (they are always
/// coming) and spikes when Amaia makes noise. At 1.0 they catch the canoe.
/// </summary>
public sealed class HeatSystem : Component
{
	[Property, Range( 0f, 0.20f )] public float DriftPerSecond { get; set; } = 0.020f;
	[Property] public SoundEvent CaughtSound { get; set; }

	public float Heat { get; private set; }
	public bool Caught { get; private set; }
	public Action OnCaught { get; set; }

	public void Reset()
	{
		Heat = 0f;
		Caught = false;
	}

	/// <summary>A missed beat = a paddle splash. The hunters hear it.</summary>
	public void AddNoise( float amount )
	{
		if ( Caught ) return;
		Heat = Math.Clamp( Heat + amount, 0f, 1f );
	}

	/// <summary>A perfect run of beats can quiet the water a little.</summary>
	public void Relieve( float amount )
	{
		Heat = Math.Clamp( Heat - amount, 0f, 1f );
	}

	protected override void OnUpdate()
	{
		if ( Caught ) return;

		Heat = Math.Clamp( Heat + DriftPerSecond * Time.Delta, 0f, 1f );

		if ( Heat >= 1f )
		{
			Caught = true;
			if ( CaughtSound is not null ) Sound.Play( CaughtSound );
			OnCaught?.Invoke();
		}
	}
}
