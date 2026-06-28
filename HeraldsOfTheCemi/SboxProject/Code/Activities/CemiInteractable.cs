using Sandbox;

namespace HeraldsOfTheCemi;

public abstract class CemiInteractable : Component
{
	[Property] public string Prompt { get; set; } = "Interact";
	[Property] public bool OnePlayerAtATime { get; set; }

	[Sync( SyncFlags.FromHost )]
	public bool IsBusy { get; protected set; }

	internal void Execute( CemiPlayerState player )
	{
		if ( player is null || !Enabled )
			return;

		if ( OnePlayerAtATime && IsBusy )
			return;

		OnUseHost( player );
	}

	protected abstract void OnUseHost( CemiPlayerState player );
}
