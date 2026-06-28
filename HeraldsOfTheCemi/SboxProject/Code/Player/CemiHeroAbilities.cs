using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class CemiHeroAbilities : Component
{
	[Property, RequireComponent] public CemiPlayerState Player { get; set; }
	[Property] public CameraComponent AimCamera { get; set; }
	[Property] public float StrikeRange { get; set; } = 650.0f;
	[Property] public float StrikeRadius { get; set; } = 28.0f;
	[Property] public float StrikeCost { get; set; } = 18.0f;
	[Property] public int StrikeDamage { get; set; } = 35;
	[Property] public float StrikeCooldown { get; set; } = 0.45f;
	[Property] public float PassiveRechargePerSecond { get; set; } = 7.0f;

	private TimeSince _timeSinceStrike;
	private TimeSince _timeSinceRecharge;

	protected override void OnUpdate()
	{
		if ( IsProxy || Player is null )
			return;

		if ( Input.Pressed( "attack1" ) )
			RequestResonanceStrike();

		if ( Input.Pressed( "power" ) )
			RequestToggleHeraldMode();

		if ( _timeSinceRecharge > 0.25f )
		{
			_timeSinceRecharge = 0;
			RequestRecharge( PassiveRechargePerSecond * 0.25f );
		}
	}

	[Rpc.Host( NetFlags.OwnerOnly )]
	private void RequestResonanceStrike()
	{
		if ( Player is null || AimCamera is null || _timeSinceStrike < StrikeCooldown )
			return;

		if ( !Player.SpendResonance( StrikeCost ) )
			return;

		_timeSinceStrike = 0;

		var start = AimCamera.WorldPosition;
		var end = start + AimCamera.WorldRotation.Forward * StrikeRange;
		var trace = Scene.Trace
			.Sphere( StrikeRadius, start, end )
			.WithoutTags( "player" )
			.Run();

		if ( trace.Hit )
		{
			var drone = trace.GameObject?.Components.Get<ApertureDrone>();
			drone?.ApplyDamageHost( Player, StrikeDamage );
		}

		PlayStrikeEffects( start, trace.Hit ? trace.EndPosition : end );
	}

	[Rpc.Host( NetFlags.OwnerOnly | NetFlags.Unreliable )]
	private void RequestRecharge( float amount )
	{
		if ( Player is null )
			return;

		Player.RestoreResonance( float.Clamp( amount, 0.0f, 3.0f ) );
	}

	[Rpc.Host( NetFlags.OwnerOnly )]
	private void RequestToggleHeraldMode()
	{
		Player?.ToggleHeraldMode();
	}

	[Rpc.Broadcast( NetFlags.HostOnly | NetFlags.Unreliable )]
	private void PlayStrikeEffects( Vector3 start, Vector3 end )
	{
		// Hook a beam, particles, camera impulse and the three-quick/one-low/two-wide audio cue here.
		DebugOverlay.Line( start, end, Color.Cyan, 0.2f );
	}
}
