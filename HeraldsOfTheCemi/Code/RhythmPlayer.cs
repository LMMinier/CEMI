using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Heralds;

/// <summary>
/// Amaia, carrying her son through the delta. Each beat she "answers" the
/// river. Accurate answers slide the canoe one node deeper toward the hidden
/// corridor under the roots; mistakes make noise that feeds Aperture's Heat.
/// </summary>
public sealed class RhythmPlayer : Component
{
	[Property] public ResonanceConductor Conductor { get; set; }
	[Property] public HeatSystem Heat { get; set; }

	/// <summary>Optional hand-placed path. If empty, a path is generated procedurally.</summary>
	[Property] public List<GameObject> PathNodes { get; set; } = new();

	/// <summary>How many nodes to generate when no PathNodes are placed.</summary>
	[Property, Range( 4, 40 )] public int ProceduralNodeCount { get; set; } = 16;

	/// <summary>How fast the canoe glides between nodes.</summary>
	[Property, Range( 1f, 20f )] public float GlideSpeed { get; set; } = 4f;

	/// <summary>A streak of this many perfects quiets the water a little.</summary>
	[Property, Range( 2, 16 )] public int ReliefCombo { get; set; } = 6;

	[Property] public SoundEvent PaddleSound { get; set; }
	[Property] public SoundEvent SplashSound { get; set; }

	public int NodeIndex { get; private set; }
	public int Combo { get; private set; }
	public int Score { get; private set; }
	public int NodeTotal => Math.Max( 1, _waypoints.Count - 1 );
	public RhythmJudgement LastJudgement { get; private set; } = RhythmJudgement.Miss;
	public float LastJudgementTime { get; private set; } = -10f;
	public bool Reached { get; private set; }

	readonly List<Vector3> _waypoints = new();
	Vector3 _targetPos;

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

		if ( PathNodes is not null && PathNodes.Count > 0 )
		{
			foreach ( var go in PathNodes.Where( g => g.IsValid() ) )
				_waypoints.Add( go.WorldPosition );
			return;
		}

		// Procedural under-root channel: a winding line snaking forward.
		int n = Math.Max( 2, ProceduralNodeCount );
		for ( int i = 0; i < n; i++ )
		{
			float t = i / (float)(n - 1);
			float x = t * 1200f;                       // forward into the delta
			float y = MathF.Sin( t * MathF.PI * 3f ) * 220f; // weaving through roots
			float z = MathF.Sin( t * MathF.PI * 6f ) * 18f;  // gentle bob
			_waypoints.Add( new Vector3( x, y, z ) );
		}
	}

	public void Reset()
	{
		NodeIndex = 0;
		Combo = 0;
		Score = 0;
		Reached = false;
		LastJudgement = RhythmJudgement.Miss;
		LastJudgementTime = -10f;

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

		var judge = Conductor.Judge( Conductor.SongTime );
		LastJudgement = judge;
		LastJudgementTime = Time.Now;

		switch ( judge )
		{
			case RhythmJudgement.Perfect:
				Combo++;
				Score += 100 + Combo * 10;
				if ( Combo % ReliefCombo == 0 ) Heat?.Relieve( 0.06f );
				Advance();
				if ( PaddleSound is not null ) Sound.Play( PaddleSound, WorldPosition );
				break;

			case RhythmJudgement.Good:
				Combo++;
				Score += 50;
				Advance();
				if ( PaddleSound is not null ) Sound.Play( PaddleSound, WorldPosition );
				break;

			case RhythmJudgement.Miss:
				Combo = 0;
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

		if ( NodeIndex >= _waypoints.Count - 1 )
			Reached = true;
	}
}
