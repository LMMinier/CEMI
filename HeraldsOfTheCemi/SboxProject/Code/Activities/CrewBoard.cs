using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class CrewBoard : CemiInteractable
{
	[Property] public string CrewName { get; set; } = "Uptown Heralds";
	[Property] public int JoinCred { get; set; } = 10;

	[Sync( SyncFlags.FromHost )] public int MemberInteractions { get; private set; }

	protected override void OnUseHost( CemiPlayerState player )
	{
		var cleaned = CemiGameRules.CleanPlayerText( CrewName, 20 );
		if ( cleaned.Length < 2 || player.CrewName == cleaned )
			return;

		player.SetCrew( cleaned );
		player.AwardCred( JoinCred );
		MemberInteractions++;
		PlayCrewJoined( player.HeroName, cleaned );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void PlayCrewJoined( string heroName, string crewName )
	{
		Log.Info( $"{heroName} joined crew {crewName}." );
	}
}
