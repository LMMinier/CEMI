using Sandbox;

namespace HeraldsOfTheCemi;

/// <summary>
/// Networked streetball activity foundation. This deliberately proves matchmaking,
/// possession, scoring, rewards and mission progress before full ball physics are added.
/// </summary>
public sealed class StreetballCourt : CemiInteractable
{
	[Property] public string CourtName { get; set; } = "Cemí Park Court";
	[Property] public int ScoreToWin { get; set; } = 11;
	[Property] public int WinBy { get; set; } = 2;
	[Property] public int WinnerCred { get; set; } = 75;

	[Sync( SyncFlags.FromHost )] public CemiPlayerState HomePlayer { get; private set; }
	[Sync( SyncFlags.FromHost )] public CemiPlayerState AwayPlayer { get; private set; }
	[Sync( SyncFlags.FromHost )] public CemiPlayerState PossessionPlayer { get; private set; }
	[Sync( SyncFlags.FromHost )] public int HomeScore { get; private set; }
	[Sync( SyncFlags.FromHost )] public int AwayScore { get; private set; }
	[Sync( SyncFlags.FromHost )] public int ShotAttempts { get; private set; }
	[Sync( SyncFlags.FromHost )] public bool MatchActive { get; private set; }
	[Sync( SyncFlags.FromHost )] public string WinnerName { get; private set; } = string.Empty;

	protected override void OnUseHost( CemiPlayerState player )
	{
		if ( !string.IsNullOrEmpty( WinnerName ) )
			ResetCourt();

		if ( HomePlayer is null )
		{
			HomePlayer = player;
			PlayCourtMessage( $"{player.HeroName} called next at {CourtName}." );
			return;
		}

		if ( AwayPlayer is null )
		{
			if ( player == HomePlayer )
				return;

			AwayPlayer = player;
			PossessionPlayer = HomePlayer;
			MatchActive = true;
			PlayCourtMessage( $"Game on: {HomePlayer.HeroName} versus {AwayPlayer.HeroName}. First to {ScoreToWin}, win by {WinBy}." );
			return;
		}

		if ( !MatchActive || player != PossessionPlayer )
			return;

		TakePrototypeShot( player );
	}

	private void TakePrototypeShot( CemiPlayerState shooter )
	{
		ShotAttempts++;

		// The vertical slice uses a predictable scoring cadence so networking and rewards
		// can be tested. Replace this with ball possession and a timing-based shot meter.
		var points = ShotAttempts % 4 == 0 ? 3 : 2;

		if ( shooter == HomePlayer )
			HomeScore += points;
		else
			AwayScore += points;

		PlayCourtMessage( $"{shooter.HeroName} scored {points}. {HomeScore}-{AwayScore}." );

		if ( HasWon( HomeScore, AwayScore ) )
		{
			CompleteMatch( HomePlayer );
			return;
		}

		if ( HasWon( AwayScore, HomeScore ) )
		{
			CompleteMatch( AwayPlayer );
			return;
		}

		PossessionPlayer = shooter == HomePlayer ? AwayPlayer : HomePlayer;
	}

	private bool HasWon( int leadingScore, int trailingScore )
	{
		return leadingScore >= int.Max( 1, ScoreToWin )
			&& leadingScore - trailingScore >= int.Max( 1, WinBy );
	}

	private void CompleteMatch( CemiPlayerState winner )
	{
		MatchActive = false;
		PossessionPlayer = null;
		WinnerName = winner?.HeroName ?? "Unknown Herald";

		if ( winner is not null )
		{
			winner.AddStreetballWin();
			winner.AwardCred( WinnerCred );
		}

		PlayCourtMessage( $"{WinnerName} won at {CourtName}, {HomeScore}-{AwayScore}." );
	}

	private void ResetCourt()
	{
		HomePlayer = null;
		AwayPlayer = null;
		PossessionPlayer = null;
		HomeScore = 0;
		AwayScore = 0;
		ShotAttempts = 0;
		MatchActive = false;
		WinnerName = string.Empty;
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void PlayCourtMessage( string message )
	{
		Log.Info( message );
	}
}
