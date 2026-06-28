using Sandbox;

namespace HeraldsOfTheCemi;

public enum MissionObjective
{
	None,
	Graffiti,
	Streetball,
	DefeatAperture,
	DiscoverDistrict
}

public static class CemiGameRules
{
	public const int MaxHealth = 100;
	public const float MaxResonance = 100.0f;
	public const float InteractionRange = 180.0f;
	public const int DefaultMissionReward = 100;

	public static string CleanPlayerText( string value, int maxLength )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return string.Empty;

		value = value.Trim();
		if ( value.Length > maxLength )
			value = value[..maxLength];

		return value;
	}
}
