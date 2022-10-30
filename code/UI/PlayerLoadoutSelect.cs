using Sandbox.UI;
using CastleDefenders.UI.Components;

public sealed class LoadoutSelect : Panel
{
	public UserSelectUI.LoadoutSelectPanel LoadoutPanel = new();

	public LoadoutSelect()
	{
		Style.Set( "position: absolute; width: 100%; height: 100%;" );
		StyleSheet.Load( "UI/Components/UserLoadout.scss" );
		AddChild( LoadoutPanel );
		LoadoutPanel.AddLoudout("Basic", 0 ,() => {} );
		LoadoutPanel.AddLoudout("LEVEL 1", 1 ,() => {} );
		LoadoutPanel.AddLoudout("LEVEL 2", 2 ,() => { } );
		LoadoutPanel.AddLoudout("LEVEL 3", 3, () => { } );
	}
}
