using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

public partial class TD2HUD : RootPanel
{
	public static TD2HUD CurrentHud;

	public Scoreboard<ScoreboardEntry> Scoreboard { get; set; }

	public TD2HUD()
	{
		CurrentHud = this;

		AddChild<ChatBox>();
		Scoreboard = AddChild<Scoreboard<ScoreboardEntry>>();

		AddChild<Status>();
		AddChild<VoiceList>();
	}
}
