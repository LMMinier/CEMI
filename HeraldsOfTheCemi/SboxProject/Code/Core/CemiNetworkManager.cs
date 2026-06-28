using Sandbox;

namespace HeraldsOfTheCemi;

/// <summary>
/// Host-side player spawning for online sessions. Place one instance in the startup scene,
/// then assign a network-ready player prefab and a spawn point in the editor.
/// </summary>
public sealed class CemiNetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }

	public void OnActive( Connection connection )
	{
		if ( PlayerPrefab is null )
		{
			Log.Warning( "CemiNetworkManager has no PlayerPrefab assigned." );
			return;
		}

		var spawnTransform = SpawnPoint is null ? Transform.World : SpawnPoint.Transform.World;
		var playerObject = PlayerPrefab.Clone( spawnTransform );
		var state = playerObject.Components.Get<CemiPlayerState>( FindMode.EverythingInSelfAndDescendants );

		if ( state is not null )
			state.HeroName = CemiGameRules.CleanPlayerText( connection.DisplayName, 20 );

		playerObject.NetworkSpawn( connection );
	}
}
