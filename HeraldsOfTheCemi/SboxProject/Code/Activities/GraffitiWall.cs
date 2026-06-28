using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class GraffitiWall : CemiInteractable
{
	[Property] public string WallName { get; set; } = "Community Mural Wall";
	[Property] public int CredReward { get; set; } = 25;
	[Property] public float RetagCooldown { get; set; } = 4.0f;

	[Sync( SyncFlags.FromHost )] public string LastTagger { get; private set; } = string.Empty;
	[Sync( SyncFlags.FromHost )] public string CrewClaim { get; private set; } = string.Empty;
	[Sync( SyncFlags.FromHost )] public int TagCount { get; private set; }

	private TimeSince _timeSinceTag;

	protected override void OnUseHost( CemiPlayerState player )
	{
		if ( _timeSinceTag < RetagCooldown )
			return;

		_timeSinceTag = 0;
		LastTagger = player.HeroName;
		CrewClaim = player.CrewName;
		TagCount++;

		player.AddGraffitiTag();
		player.AwardCred( CredReward );
		PlayTagEffects( player.HeroName, player.CrewName );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void PlayTagEffects( string heroName, string crewName )
	{
		// Replace this with a legal-wall decal picker and an authored spray animation.
		Log.Info( $"{heroName} tagged {WallName} for {crewName}." );
	}
}
