using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Components.Qmenu
{
	public class QMenu : Panel
	{
		public bool toggle_open = false;
		public TimeSince lastKeyPress;
		private Panel PNLleft;
		//private Panel PNLright;
		public Panel PNLleftInner;
		public Panel PNLrightInner;
		/// <summary>
		/// Make a quick menu like a debug menu or a settings menu for your game
		/// </summary>
		public QMenu()
		{
			StyleSheet.Load( "UI/QMenu/QMenu.scss" );
			PNLleft = Add.Panel( "PNL-L PNL" );
			PNLleftInner = PNLleft.Add.Panel("");
			//Panel v = Add.Panel( "column" );
			//v.Add.Label( "Quick Menu Component!", "title" );
			//v.Add.Label( "Label classes:", "" );
			//v.Add.Label( "class: warn", "warn" );
			//v.Add.Label( "class: err", "err" );
			//PNLleft.AddChild( v );
		}

		public Panel PanelLeft
		{
			get { return PNLleftInner; }
			set { PNLleftInner = value;}
		}

		public override void Tick()
		{
			base.Tick();

			if (Input.Down(InputButton.Menu) && lastKeyPress > 0.3)
			{
				lastKeyPress = 0;
				toggle_open = !toggle_open;
				SetClass( "open", toggle_open );
			}
		}
		public class Actions
		{
			public class action_Button : Panel
			{
				public Button btn;

				public action_Button( String Text, String ExtraClasses ,Action OnClick)
				{
					btn = Add.Button( Text, "btn-action " + ExtraClasses, () =>
					{
						if (OnClick != null)
						{
							OnClick();
						}
					} );
				}
			}

		}
		public class Categorise : Panel
		{
			public class SectionHeader : Panel
			{
				public Label HeaderText;

				public SectionHeader(string Text)
				{
					HeaderText = Add.Label( Text, "header-Text" );
				}
			}
			public class SectionBox : Panel
			{

			}
		}

		public class Container : Panel
		{
			public class Normal : Panel
			{
				public Panel pnl;
				public Normal()
				{
					pnl = Add.Panel();
				}
			}
			public class Warning : Panel
			{
				public Panel pnl;
				public Warning()
				{
					pnl = Add.Panel();
				}
			}
			public class Alert : Panel
			{
				public Panel pnl;
				public Alert()
				{
					pnl = Add.Panel();
				}
			}
		}
	}
}
