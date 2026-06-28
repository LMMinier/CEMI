using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class CemiPlayerState : Component
{
	[Sync( SyncFlags.FromHost )] public string HeroName { get; set; } = "Herald";
	[Sync( SyncFlags.FromHost )] public int Health { get; set; } = CemiGameRules.MaxHealth;
	[Sync( SyncFlags.FromHost )] public float Resonance { get; set; } = CemiGameRules.MaxResonance;
	[Sync( SyncFlags.FromHost )] public int StreetCred { get; set; }
	[Sync( SyncFlags.FromHost )] public string CrewName { get; set; } = string.Empty;
	[Sync( SyncFlags.FromHost )] public string CurrentDistrict { get; set; } = "Unmapped";
	[Sync( SyncFlags.FromHost )] public int GraffitiTags { get; set; }
	[Sync( SyncFlags.FromHost )] public int StreetballWins { get; set; }
	[Sync( SyncFlags.FromHost )] public bool HeraldMode { get; set; }

	[Sync( SyncFlags.FromHost )] public string ActiveMissionId { get; set; } = string.Empty;
	[Sync( SyncFlags.FromHost )] public string MissionTitle { get; set; } = string.Empty;
	[Sync( SyncFlags.FromHost )] public MissionObjective MissionObjective { get; set; }
	[Sync( SyncFlags.FromHost )] public int MissionProgress { get; set; }
	[Sync( SyncFlags.FromHost )] public int MissionTarget { get; set; }
	[Sync( SyncFlags.FromHost )] public int MissionReward { get; set; }
	[Sync( SyncFlags.FromHost )] public bool MissionComplete { get; set; }

	public void TryUse( CemiInteractable target )
	{
		if ( IsProxy || target is null )
			return;

		RequestUse( target );
	}

	public void RequestCrew( string crewName )
	{
		if ( IsProxy )
			return;

		RequestCrewHost( crewName );
	}

	public void RequestHeroName( string heroName )
	{
		if ( IsProxy )
			return;

		RequestHeroNameHost( heroName );
	}

	[Rpc.Host( NetFlags.OwnerOnly )]
	private void RequestUse( CemiInteractable target )
	{
		if ( target is null || !target.Enabled )
			return;

		if ( WorldPosition.Distance( target.WorldPosition ) > CemiGameRules.InteractionRange )
			return;

		target.Execute( this );
	}

	[Rpc.Host( NetFlags.OwnerOnly )]
	private void RequestCrewHost( string requestedName )
	{
		var cleaned = CemiGameRules.CleanPlayerText( requestedName, 20 );
		if ( cleaned.Length < 2 )
			return;

		SetCrew( cleaned );
	}

	[Rpc.Host( NetFlags.OwnerOnly )]
	private void RequestHeroNameHost( string requestedName )
	{
		var cleaned = CemiGameRules.CleanPlayerText( requestedName, 20 );
		if ( cleaned.Length < 2 )
			return;

		HeroName = cleaned;
	}

	internal void SetCrew( string crewName )
	{
		CrewName = CemiGameRules.CleanPlayerText( crewName, 20 );
	}

	internal void AwardCred( int amount )
	{
		StreetCred = int.Max( 0, StreetCred + amount );
	}

	internal bool SpendResonance( float amount )
	{
		if ( amount <= 0.0f || Resonance < amount )
			return false;

		Resonance = float.Clamp( Resonance - amount, 0.0f, CemiGameRules.MaxResonance );
		return true;
	}

	internal void RestoreResonance( float amount )
	{
		Resonance = float.Clamp( Resonance + amount, 0.0f, CemiGameRules.MaxResonance );
	}

	internal void ApplyDamage( int amount )
	{
		Health = int.Clamp( Health - int.Max( 0, amount ), 0, CemiGameRules.MaxHealth );
	}

	internal void Heal( int amount )
	{
		Health = int.Clamp( Health + int.Max( 0, amount ), 0, CemiGameRules.MaxHealth );
	}

	internal void ToggleHeraldMode()
	{
		HeraldMode = !HeraldMode;
	}

	internal void StartMission( string id, string title, MissionObjective objective, int target, int reward )
	{
		ActiveMissionId = CemiGameRules.CleanPlayerText( id, 48 );
		MissionTitle = CemiGameRules.CleanPlayerText( title, 64 );
		MissionObjective = objective;
		MissionProgress = 0;
		MissionTarget = int.Max( 1, target );
		MissionReward = int.Max( 0, reward );
		MissionComplete = false;
	}

	internal void RecordMissionProgress( MissionObjective objective, int amount = 1 )
	{
		if ( MissionComplete || MissionObjective != objective || string.IsNullOrEmpty( ActiveMissionId ) )
			return;

		MissionProgress = int.Clamp( MissionProgress + int.Max( 0, amount ), 0, MissionTarget );
		MissionComplete = MissionProgress >= MissionTarget;
	}

	internal bool ClaimMissionReward( string missionId )
	{
		if ( !MissionComplete || ActiveMissionId != missionId )
			return false;

		AwardCred( MissionReward );
		ActiveMissionId = string.Empty;
		MissionTitle = string.Empty;
		MissionObjective = MissionObjective.None;
		MissionProgress = 0;
		MissionTarget = 0;
		MissionReward = 0;
		MissionComplete = false;
		return true;
	}

	internal void AddGraffitiTag()
	{
		GraffitiTags++;
		RecordMissionProgress( MissionObjective.Graffiti );
	}

	internal void AddStreetballWin()
	{
		StreetballWins++;
		RecordMissionProgress( MissionObjective.Streetball );
	}

	internal void SetDistrict( string districtName )
	{
		var cleaned = CemiGameRules.CleanPlayerText( districtName, 32 );
		if ( string.IsNullOrEmpty( cleaned ) || CurrentDistrict == cleaned )
			return;

		CurrentDistrict = cleaned;
		RecordMissionProgress( MissionObjective.DiscoverDistrict );
	}
}
