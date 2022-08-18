using System;
using Sandbox;
public partial class CDGame
{
	//Basic Gameplay and wave statuses
	public enum GameEnum
	{
		Idle,
		Voting,
		Active,
		Post
	}

	public enum WaveEnum
	{
		PreWave,
		ActiveWave,
		PostWave,
	}

	//Difficulty
	public enum DiffEnum
	{
		Easy,
		Medium,
		Hard,
		VeryHard
	}

	//To add towards difficulty, players can opt to choose a difficulty variant
	public enum DiffVariants
	{
		None,
		//TODO: Think of difficulty variants
	}

	public GameEnum GameStatus;
	public WaveEnum WaveStatus;
	public DiffVariants DifficultyVariant;

	public void InitVoting()
	{
		GameStatus = GameEnum.Voting;
	}

}
