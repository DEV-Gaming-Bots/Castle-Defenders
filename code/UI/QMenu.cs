using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Components.Qmenu; // <-- Important for the debug menu

public class DebugMenu : RootPanel
{
	public DebugMenu()
	{
		// Debug menu template
		QMenu q = new();
		Panel MainPanel = Add.Panel( "panel-inner" );
		q.PanelLeft.AddChild( MainPanel );
		AddChild( q );
		StyleSheet.Load( "UI/dropdown.scss" );

		DropDown dropd = new( );
		dropd.Options.Add( new Option() { Title = "Item 1", Value="1"} );
		dropd.Options.Add( new Option() { Title = "Item 2", Value = "2" } );
		dropd.Options.Add( new Option() { Title = "Item 3", Value = "3" } );
		//
		QMenu.Categorise.SectionHeader header1 = new("Debug Menu");
		QMenu.Categorise.SectionHeader header1_messages = new("Messages");
		// Normal message box
		QMenu.Container.Normal normal = new();
		normal.Add.Label( "This is just an test", "test" );
		// warning message box
		QMenu.Container.Warning warning = new();
		warning.Add.Label( "Warning: There is no warning within this box.", "test b" );
		// alert message box
		QMenu.Container.Alert alert = new();
		alert.Add.Label( "Error: no error in this box... yet.", "test b" );
		// Section header, acts like seperator 
		QMenu.Categorise.SectionHeader header2_buttonTests = new( "Buttons" );
		QMenu.Categorise.SectionHeader header3_dropdown = new( "Dropdown ( Built-In )" );
		// Adding the elements to the main panel
		MainPanel.AddChild( header1 );
		MainPanel.AddChild( header1_messages );
		MainPanel.AddChild( normal );
		MainPanel.AddChild( warning );
		MainPanel.AddChild( alert );
		MainPanel.AddChild( header2_buttonTests );

		QMenu.Actions.action_Button btn = new( "Button", null, () =>
		{
			Log.Info( "Action!" );
		} );
		QMenu.Actions.action_Button btn_in_warningbox = new( "Button", null, () =>
		{
			Log.Info( "Action!" );
		} );
		normal.AddChild( btn_in_warningbox );
		MainPanel.AddChild( btn );
		MainPanel.AddChild( header3_dropdown );
		MainPanel.AddChild( dropd );
		// Remove the main menu and the tower menu
		//MainMenu m = new();
		//AddChild(m);
		//sboxtowerui.CondoTower condos = new sboxtowerui.CondoTower();
		//AddChild(condos);

	}
}
