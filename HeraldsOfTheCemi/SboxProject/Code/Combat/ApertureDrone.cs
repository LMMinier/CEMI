using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class ApertureDrone : Component
{
	[Property] public int MaxHealth { get; set; } = 100;
	[Property] public int DefeatCred { get; set; } = 40;

	[Sync( SyncFlags.FromHost )] public int Health { get; private set; } = 100;
	[Sync( SyncFlags.FromHost )] public bool Defeated { get; private set; }

	protected override void OnStart()
	{
		Health = int.Max( 1, MaxHealth );
	}

	internal void ApplyDamageHost( CemiPlayerState attacker, int amount )
	{
		if ( Defeated || attacker is null || amount <= 0 )
			return;

		Health = int.Max( 0, Health - amount );
		PlayDamageEffects( WorldPosition );

		if ( Health > 0 )
			return;

		Defeated = true;
		attacker.AwardCred( DefeatCred );
		attacker.RecordMissionProgress( MissionObjective.DefeatAperture );
		PlayDefeatEffects( WorldPosition );
	}

	[Rpc.Broadcast( NetFlags.HostOnly | NetFlags.Unreliable )]
	private void PlayDamageEffects( Vector3 position )
	{
		DebugOverlay.Sphere( position, 18.0f, Color.Orange, 0.15f );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void PlayDefeatEffects( Vector3 position )
	{
		DebugOverlay.Sphere( position, 42.0f, Color.Cyan, 0.6f );
	}
}
