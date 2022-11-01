using System;
using System.Linq;
using Sandbox;
using static PlayerLoadout;

public sealed partial class CDPawn : IPlayerData
{
	//OldEXP is used to display animations for the client
	//The rest is self-explanatory 
	[Net] public int OldEXP { get; private set; }
	[Net] public int Cash { get; private set; }
	[Net] public int EXP { get; set; }
	[Net] public int ReqEXP { get; set; }
	[Net] public int Level { get; set; }

	[Net, Local] public string[] TowerSlots { get; set; }

	[ClientRpc]
	public void UpdateSlots(string item, int slot)
	{
		AddSlot( new Slot( item, slot ) );
	}

	[ClientRpc]
	public void ChangeSlot(string name, int slot)
	{
		ReplaceSlot( slot, name );
	}

	public void NewPlayerStats()
	{
		Level = 1;
		EXP = 0;
		OldEXP = 0;
		ReqEXP = 250;

		TowerSlots = new[]
		{
			"Pistol",
			"SMG",
			"Sniper",
			"Trickster",
			"RadioactiveEmitter",
			"Lightning",
			"TimeDisplacer"
		};

		CDGame.Instance.SaveData( this );
	}

	public void LoadStats(IPlayerData playerData)
	{
		Level = playerData.Level;
		EXP = playerData.EXP;
		ReqEXP = playerData.ReqEXP;
		TowerSlots = playerData.TowerSlots;
	}

	[Net]
	public string GetSlotNet { get; private set; }

	public string GetSlotIndex(int index)
	{
		return "Pistol";
	}

	public void SetUpPlayer()
	{
		if ( CDGame.Instance.LoopedTimes > 1 )
			return;

		Cash = 60 + (Level - 1) * 10;

		SelectedTower?.Delete();
		SelectedTower = null;

		if ( CDGame.Instance.Competitive )
		{
			while(CurTeam == TeamEnum.Unknown)
			{
				var blueCount = All.OfType<CDPawn>().Count( x => x.CurTeam == TeamEnum.Blue );
				var redCount = All.OfType<CDPawn>().Count( x => x.CurTeam == TeamEnum.Red );
				switch (Rand.Int(1, 2))
				{
					case 1:
						if( blueCount > redCount )
						{
							CurTeam = TeamEnum.Red;
							break;
						}
						CurTeam = TeamEnum.Blue;
						break;

					case 2:
						if ( redCount > blueCount )
						{
							CurTeam = TeamEnum.Blue;
							break;
						}

						CurTeam = TeamEnum.Red;
						break;
				}
			}

			OnOtherTeamSide = false;

			if ( CurTeam == TeamEnum.Red )
				Transform = All.OfType<OpposingSpawnpoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault().Transform;
		}
	}

	public void AddCash(int addCash)
	{
		Cash += addCash;
		Cash = Cash.Clamp( 0, 5000 );
	}

	public void TakeCash( int subCash )
	{
		Cash -= subCash;
	}

	public void AddEXP(int addEXP)
	{
		EXP += addEXP;

		if(EXP >= ReqEXP)
		{
			PlaySoundOnClient( To.Single( this ), "level_up" );

			//Incase the EXP goes over the next EXP requirements
			while ( EXP >= ReqEXP )
			{
				Level++;
				EXP -= ReqEXP;
				ReqEXP += (Level - 1) * 250;
			}
		}
	}
	public int GetCash()
	{
		return Cash;
	}

	public int GetEXP()
	{
		return EXP;
	}

	public int GetLevel()
	{
		return Level;
	}

	public int GetOldEXP()
	{
		return OldEXP;
	}

	public int GetReqEXP()
	{
		return ReqEXP;
	}
}

