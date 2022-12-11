using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Components.popup
{
	public enum PopupVertical
	{
		Top,
		Center,
		Bottom,
	};

	public enum PopupHorizontal
	{
		Left,
		Center,
		Right
	}

	public partial class WindowPopup : Panel
	{
		public string SCSS;

		public Panel body;
		public Label title;
		public Panel Header;
		public Panel content;
		public Panel footer;
		// vertical and horizontal 
		public PopupVertical _Vertical = PopupVertical.Center;
		public PopupHorizontal _Horizontal = PopupHorizontal.Center;

		public WindowPopup( string title, float timer, PopupVertical Vertical = PopupVertical.Center, PopupHorizontal Horizontal = PopupHorizontal.Center )
		{
			_Vertical = Vertical;
			_Horizontal = Horizontal;

			StyleSheet.Load( SCSS != null ? SCSS : "UI/Components/BasePopup.scss" );

			AddClass( "popup-body" );
			
			/* check if vertical is top center or bottom */
			switch( Vertical )
			{
				case PopupVertical.Top: AddClass( "v-popup-top" ); break;
				case PopupVertical.Center: AddClass( "v-popup-center" ); break;
				case PopupVertical.Bottom: AddClass( "v-popup-bottom" ); break;
				default: AddClass( "v-popup-center" ); Log.Warning( "No Vertical Alignment" ); break;
			}

			/* check if horizontal is left center or right */
			switch ( Horizontal )
			{
				case PopupHorizontal.Left: AddClass( "h-popup-left" ); break;
				case PopupHorizontal.Center: AddClass( "h-popup-center " ); break;
				case PopupHorizontal.Right: AddClass( "h-popup-right " ); break;
				default: AddClass( "h-popup-center" ); Log.Warning( "No Horizontal Alignment" ); break;
			}

			body = Add.Panel( "body" );
			Header = body.Add.Panel( "header" );
			this.title = Header.Add.Label( title, "title" );
			content = body.Add.Panel( "content" );
			footer = body.Add.Panel( "footer" );

			_ = LifeTime(timer);
		}

		[ClientRpc]
		public static void CreatePopUp( string title, float timer = 1.0f, PopupVertical Vertical = PopupVertical.Center, PopupHorizontal Horizontal = PopupHorizontal.Center )
		{
			var popup = new WindowPopup(title, timer, Vertical, Horizontal);
			CDHUD.CurrentHud.AddChild( popup );
		}

		public async Task LifeTime(float time)
		{
			await Task.DelaySeconds( time );
			this?.Delete();
		}
	}


}
