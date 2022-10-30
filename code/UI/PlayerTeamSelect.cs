using System;
using Sandbox.UI;
using Sandbox.UI.Construct;

public sealed class PlayerTeamSelect : Panel
{
	public Panel BlueTeam;
	public Panel RedTeam;
	public Panel GameInfoPanel;
	public Label DifficultyText;

	public PlayerTeamSelect( Action onBlueClick, Action onRedClick )
	{
		StyleSheet.Load( "UI/PlayerTeamSelect.scss" );

		// gameinfo
		GameInfoPanel = Add.Panel( "gameinfo" );
		GameInfoPanel.Add.Label( "TEAM SELECT", "big text" );
		DifficultyText = GameInfoPanel.Add.Label( "Difficulty: -", "text" );
		// gameinfo end

		var centerPanel = Add.Panel( "centerPanel" );
		BlueTeam = centerPanel.Add.Panel( "blue" );
		BlueTeam.Add.Label( "BLUE", "text" );
		BlueTeam.AddEventListener( "onClick", () =>
		{
			onBlueClick();
		} );
		///////// ADD GAMEINFO PANEL //////////
		centerPanel.AddChild( GameInfoPanel );
		///////////////////////////////////////
		RedTeam = centerPanel.Add.Panel( "red" );
		RedTeam.Add.Label( "RED", "text" );
		RedTeam.AddEventListener( "onClick", () =>
		{
			onRedClick();
		} );
	}
}
