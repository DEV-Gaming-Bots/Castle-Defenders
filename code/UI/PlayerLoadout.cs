using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

public  class PlayerLoadout : Panel
{
	public static Panel Slots;
	private static int _lastSlot = -1;

	public PlayerLoadout()
	{
		StyleSheet.Load( "UI/PlayerLoadout.scss" );
		Slots = Add.Panel( "Slots" );

		ConsoleSystem.Run( "cd_get_towerslots" );
	}

	public static void ClearSlots()
	{
		Slots.DeleteChildren();
	}

	public static void AddSlot( Slot s )
	{
		Slots.AddChild( s );
	}

	public  class Slot : Panel
	{
		public Slot( string slotName, int slotNum )
		{
			var slot = Add.Panel( "slot" );
			slot.SetClass( slotName, true );
			slot.Add.Label( $" {slotNum} ", "slot-num" );
		}
	}

	public static void ReplaceSlot(int slotNum, string slotName)
	{
		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
			return;

		//	Slots.GetChild( slotNum ).GetChild( 0 ).Set
		Log.Info( slotName );
		Log.Info( slotNum );
		Slots.GetChild( slotNum ).GetChild( 0 ).SetClass( slotName, true );
	}

	public static void SetSlot( int slotNum )
	{
		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
			return;

		if ( _lastSlot != -1 )
			Slots.GetChild( _lastSlot ).GetChild( 0 ).SetClass( "selected", false );

		if( slotNum >= 0 )
			Slots.GetChild( slotNum ).GetChild( 0 ).SetClass( "selected", true );

		_lastSlot = slotNum;
	}
}
