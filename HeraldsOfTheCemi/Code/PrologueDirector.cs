using Sandbox;
using System.Linq;

namespace Heralds;

public enum PrologueState
{
	Playing,
	Escaped,
	Caught,
}

/// <summary>
/// Runs the prologue vertical slice: "Before the Corridors".
/// Win  = reach the corridor node before Aperture catches you.
/// Lose = Heat reaches 1.0.
/// Press [R] (reload) on an end screen to run it again.
/// </summary>
public sealed class PrologueDirector : Component
{
	[Property] public RhythmPlayer Player { get; set; }
	[Property] public HeatSystem Heat { get; set; }
	[Property] public ResonanceConductor Conductor { get; set; }

	public PrologueState State { get; private set; } = PrologueState.Playing;

	protected override void OnStart()
	{
		Player ??= Scene.GetAllComponents<RhythmPlayer>().FirstOrDefault();
		Heat ??= Scene.GetAllComponents<HeatSystem>().FirstOrDefault();
		Conductor ??= Scene.GetAllComponents<ResonanceConductor>().FirstOrDefault();

		if ( Heat is not null )
			Heat.OnCaught = () => State = PrologueState.Caught;
	}

	protected override void OnUpdate()
	{
		if ( State != PrologueState.Playing )
		{
			if ( Input.Pressed( "reload" ) ) Restart();
			return;
		}

		if ( Player is not null && Player.Reached )
			State = PrologueState.Escaped;
	}

	void Restart()
	{
		Conductor?.Reset();
		Heat?.Reset();
		Player?.Reset();
		State = PrologueState.Playing;
	}
}
