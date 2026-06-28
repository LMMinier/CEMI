using Sandbox;

namespace HeraldsOfTheCemi;

public sealed class CemiInteractionController : Component, PlayerController.IEvents
{
	[Property, RequireComponent] public CemiPlayerState Player { get; set; }

	public Component GetUsableComponent( GameObject gameObject )
	{
		return gameObject?.Components.Get<CemiInteractable>();
	}

	public void StartPressing( Component target )
	{
		if ( IsProxy || Player is null )
			return;

		if ( target is CemiInteractable interactable )
			Player.TryUse( interactable );
	}

	public void StopPressing( Component target )
	{
	}

	public void FailPressing()
	{
	}
}
