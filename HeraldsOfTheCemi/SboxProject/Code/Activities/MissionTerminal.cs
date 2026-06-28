using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class MissionTerminal : CemiInteractable
{
	[Property] public string MissionId { get; set; } = "bronx_first_signal";
	[Property] public string MissionTitle { get; set; } = "The First Signal";
	[Property] public MissionObjective Objective { get; set; } = MissionObjective.DefeatAperture;
	[Property] public int TargetAmount { get; set; } = 3;
	[Property] public int RewardCred { get; set; } = CemiGameRules.DefaultMissionReward;

	protected override void OnUseHost( CemiPlayerState player )
	{
		if ( player.ActiveMissionId == MissionId )
		{
			if ( player.ClaimMissionReward( MissionId ) )
				PlayMissionClaimed( player.HeroName, MissionTitle );

			return;
		}

		if ( !string.IsNullOrEmpty( player.ActiveMissionId ) )
			return;

		player.StartMission( MissionId, MissionTitle, Objective, TargetAmount, RewardCred );
		PlayMissionAccepted( player.HeroName, MissionTitle );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void PlayMissionAccepted( string heroName, string title )
	{
		Log.Info( $"{heroName} accepted mission: {title}." );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void PlayMissionClaimed( string heroName, string title )
	{
		Log.Info( $"{heroName} completed mission: {title}." );
	}
}
