using Sandbox;
using System.Linq;

namespace Heralds;

/// <summary>
/// THE ONLY THING YOU NEED TO PLACE.
///
/// Drop this component on a single empty GameObject in any scene and press
/// Play. It builds the entire prologue rhythm slice in code — conductor,
/// heat, player, director, camera, and HUD — so you don't have to hand-wire
/// anything. Replace with authored scene objects once the feel is dialed in.
/// </summary>
public sealed class PrologueBootstrap : Component
{
	[Property, Range( 40, 200 )] public float Bpm { get; set; } = 84f;
	[Property, Range( 4, 40 )] public int NodeCount { get; set; } = 16;

	protected override void OnStart()
	{
		// --- Core systems live on this GameObject ---
		var conductor = Components.GetOrCreate<ResonanceConductor>();
		conductor.Bpm = Bpm;

		var heat = Components.GetOrCreate<HeatSystem>();

		var player = Components.GetOrCreate<RhythmPlayer>();
		player.Conductor = conductor;
		player.Heat = heat;
		player.ProceduralNodeCount = NodeCount;

		var director = Components.GetOrCreate<PrologueDirector>();
		director.Player = player;
		director.Heat = heat;
		director.Conductor = conductor;

		// --- A camera so the viewport isn't black (night-river tone) ---
		if ( !Scene.GetAllComponents<CameraComponent>().Any() )
		{
			var camGo = new GameObject( true, "Camera" );
			var cam = camGo.Components.Create<CameraComponent>();
			cam.BackgroundColor = new Color( 0.02f, 0.05f, 0.07f );
			cam.ZNear = 1f;
			cam.ZFar = 10000f;
			camGo.WorldPosition = new Vector3( -400, 0, 320 );
			camGo.WorldRotation = Rotation.LookAt( new Vector3( 1, 0, -0.4f ) );
		}

		// --- Screen-space HUD ---
		var hudGo = new GameObject( true, "HUD" );
		hudGo.Components.Create<ScreenPanel>();
		hudGo.Components.Create<RhythmHud>();
	}
}
