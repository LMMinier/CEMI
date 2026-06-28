using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Heralds;

/// <summary>
/// Amaia, carrying her son through the delta. Each beat she answers the river.
/// Accurate answers slide the canoe toward the hidden corridor; mistakes make
/// noise that feeds Aperture's Heat.
/// </summary>
public sealed class RhythmPlayer : Component
{
	static readonly string[] PatternNames =
	{
		"QUICK", "QUICK", "QUICK", "LOW", "WIDE", "WIDE"
	};

	[Property] public ResonanceConductor Conductor { get; set; }
	[Property] public HeatSystem Heat { get; set; }
	[Property] public List<GameObject> PathNodes { get; set; } = new();
	[Property, Range( 4, 40 )] public int ProceduralNodeCount { get; set; } = 18;
	[Property, Range( 1f, 20f )] public float GlideSpeed { get; set; } = 5f;
	[Property, Range( 2, 16 )] public int ReliefCombo { get; set; } = 6;
	[Property] public SoundEvent PaddleSound { get; set; }
	[Property] public SoundEvent SplashSound { get; set; }

	public int NodeIndex { get; private set; }
	public int Combo { get; private set; }
	public int BestCombo { get; private set; }
	public int Score { get; private set; }
	public int PerfectCount { get; private set; }
	public int GoodCount { get; private set; }
	public int MissCount { get; private set; }
	public int Attempts => PerfectCount + GoodCount + MissCount;
	public int NodeTotal => Math.Max( 1, _waypoints.Count - 1 );
	public float Progress => Math.Clamp( NodeIndex / (float)NodeTotal, 0f, 1f );
	public float Accuracy => Attempts == 0 ? 1f : (PerfectCount + GoodCount * 0.6f) / Attempts;
	public string Rank => Accuracy switch
	{
		>= 0.95f => "S",
		>= 0.85f => "A",
		>= 0.72f => "B",
		>= 0.58f => "C",
		_ => "D"
	};
	public RhythmJudgement LastJudgement { get; private set; } = RhythmJudgement.Miss;
	public float LastJudgementTime { get; private set; } = -10f;
	public bool Reached { get; private set; }
	public string CurrentPatternName => PatternNames[Math.Abs( NodeIndex ) % PatternNames.Length];
	public int PatternStep => Math.Abs( NodeIndex ) % PatternNames.Length;

	readonly List<Vector3> _waypoints = new();
	Vector3 _targetPos;
	int _lastAnsweredBeat = -1;

	protected override void OnStart()
	{
		Conductor ??= Scene.GetAllComponents<ResonanceConductor>().FirstOrDefault();
		Heat ??= Scene.GetAllComponents<HeatSystem>().FirstOrDefault();
		BuildWaypoints();
		Reset();
	}

	void BuildWaypoints()
	{
		_waypoints.Clear();

		if ( PathNodes is not null && PathNodes.Count > 1 )
		{
			foreach ( var go in PathNodes.Where( g => g.IsValid() ) )
				_waypoints.Add( go.WorldPosition );
			return;
		}

		int n = Math.Max( 4, ProceduralNodeCount );
		for ( int i = 0; i < n; i++ )
		{
			float t = i / (float)(n - 1);
			float x = t * 1400f;
			float y = MathF.Sin( t * MathF.PI * 3f ) * 220f;
			float z = MathF.Sin( t * MathF.PI * 6f ) * 18f;
			_waypoints.Add( new Vector3( x, y, z ) );
		}
	}

	public void Reset()
	{
		NodeIndex = 0;
		Combo = 0;
		BestCombo = 0;
		Score = 0;
		PerfectCount = 0;
		GoodCount = 0;
		MissCount = 0;
		Reached = false;
		LastJudgement = RhythmJudgement.Miss;
		LastJudgementTime = -10f;
		_lastAnsweredBeat = -1;

		if ( _waypoints.Count > 0 )
		{
			_targetPos = _waypoints[0];
			WorldPosition = _targetPos;
		}
	}

	protected override void OnUpdate()
	{
		WorldPosition = Vector3.Lerp( WorldPosition, _targetPos, Time.Delta * GlideSpeed );
		if ( Reached ) return;

		if ( Input.Pressed( "jump" ) || Input.Pressed( "attack1" ) )
			Answer();
	}

	void Answer()
	{
		if ( Conductor is null ) return;

		// One answer per river beat. This prevents mouse/space mashing from
		// turning a timing game into a brute-force input game.
		int nearestBeat = (int)MathF.Round( Conductor.SongTime / Conductor.SecondsPerBeat );
		if ( nearestBeat == _lastAnsweredBeat ) return;
		_lastAnsweredBeat = nearestBeat;

		var judge = Conductor.Judge( Conductor.SongTime );
		LastJudgement = judge;
		LastJudgementTime = Time.Now;

		switch ( judge )
		{
			case RhythmJudgement.Perfect:
				PerfectCount++;
				Combo++;
				BestCombo = Math.Max( BestCombo, Combo );
				Score += 120 + Combo * 12;
				if ( Combo % ReliefCombo == 0 ) Heat?.Relieve( 0.08f );
				Advance();
				if ( PaddleSound is not null ) Sound.Play( PaddleSound, WorldPosition );
				break;

			case RhythmJudgement.Good:
				GoodCount++;
				Combo++;
				BestCombo = Math.Max( BestCombo, Combo );
				Score += 60 + Combo * 4;
				Advance();
				if ( PaddleSound is not null ) Sound.Play( PaddleSound, WorldPosition );
				break;

			case RhythmJudgement.Miss:
				MissCount++;
				Combo = 0;
				Score = Math.Max( 0, Score - 25 );
				Heat?.AddNoise( 0.12f );
				if ( SplashSound is not null ) Sound.Play( SplashSound, WorldPosition );
				break;
		}
	}

	void Advance()
	{
		if ( NodeIndex >= _waypoints.Count - 1 )
		{
			Reached = true;
			return;
		}

		NodeIndex++;
		_targetPos = _waypoints[NodeIndex];
		if ( NodeIndex >= _waypoints.Count - 1 ) Reached = true;
	}
}
