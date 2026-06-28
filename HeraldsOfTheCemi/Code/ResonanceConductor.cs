using Sandbox;
using System;

namespace Heralds;

/// <summary>
/// The river's pulse. Everything in the prologue keeps time to this.
/// "The river did not just carry water. It carried instructions."
/// </summary>
public sealed class ResonanceConductor : Component
{
	[Property, Range( 40, 200 )] public float Bpm { get; set; } = 84f;

	/// <summary>How close (in seconds) a tap must be to a beat to count as Perfect.</summary>
	[Property, Range( 0.02f, 0.30f )] public float PerfectWindow { get; set; } = 0.08f;

	/// <summary>The wider window that still counts as a Good hit.</summary>
	[Property, Range( 0.05f, 0.50f )] public float GoodWindow { get; set; } = 0.18f;

	/// <summary>Optional beat sound — a low drum / heartbeat under everything.</summary>
	[Property] public SoundEvent BeatSound { get; set; }

	public float SecondsPerBeat => 60f / MathF.Max( 1f, Bpm );

	/// <summary>Seconds since the conductor started.</summary>
	public float SongTime { get; private set; }

	/// <summary>Whole beats elapsed since start.</summary>
	public int BeatCount { get; private set; }

	/// <summary>0..1 phase through the current beat (0 = exactly on the beat).</summary>
	public float BeatPhase => (SongTime % SecondsPerBeat) / SecondsPerBeat;

	/// <summary>Fires once per beat. Hook VFX / camera pulses here.</summary>
	public Action<int> OnBeat { get; set; }

	protected override void OnStart() => Reset();

	public void Reset()
	{
		SongTime = 0f;
		BeatCount = 0;
	}

	protected override void OnUpdate()
	{
		SongTime += Time.Delta;

		int beatsNow = (int)(SongTime / SecondsPerBeat);
		while ( beatsNow > BeatCount )
		{
			BeatCount++;
			OnBeat?.Invoke( BeatCount );
			if ( BeatSound is not null )
				Sound.Play( BeatSound );
		}
	}

	/// <summary>Absolute timing error (seconds) of an input vs. the nearest beat.</summary>
	public float TimingError( float atTime )
	{
		float phase = (atTime % SecondsPerBeat) / SecondsPerBeat; // 0..1
		float dist = MathF.Min( phase, 1f - phase ) * SecondsPerBeat;
		return dist;
	}

	public RhythmJudgement Judge( float atTime )
	{
		float err = TimingError( atTime );
		if ( err <= PerfectWindow ) return RhythmJudgement.Perfect;
		if ( err <= GoodWindow ) return RhythmJudgement.Good;
		return RhythmJudgement.Miss;
	}
}

public enum RhythmJudgement
{
	Miss,
	Good,
	Perfect,
}
