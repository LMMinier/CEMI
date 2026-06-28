using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class DistrictMarker : CemiInteractable
{
	[Property] public string DistrictName { get; set; } = "Cemí Plaza";
	[Property] public int DiscoveryCred { get; set; } = 15;

	[Sync( SyncFlags.FromHost )] public string LastDiscoverer { get; private set; } = string.Empty;
	[Sync( SyncFlags.FromHost )] public int Discoveries { get; private set; }

	protected override void OnUseHost( CemiPlayerState player )
	{
		if ( player.CurrentDistrict == DistrictName )
			return;

		player.SetDistrict( DistrictName );
		player.AwardCred( DiscoveryCred );
		LastDiscoverer = player.HeroName;
		Discoveries++;
		PlayDiscovery( player.HeroName, DistrictName );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void PlayDiscovery( string heroName, string districtName )
	{
		Log.Info( $"{heroName} discovered {districtName}." );
	}
}
