using UnityEngine;

public class GameManager : MonoBehaviour
{
	public Player Player;
	public BoardManager Board;
	public UIManager UI;

	public int CurrentFloor = 0;

	private int _startingHP;
	private int _startingDamage;
	private int _startingHPItems;
	private int _startingSpeedItems;

    // Start is called before the first frame update
    void Start()
    {
		Player.ReachedExit += OnPlayerReachedExit;
		Player.DrunkardDead += OnPlayerDead;

		UI.StartGameButton.onClick.AddListener(OnStartGameClicked);
		UI.BackToMainMenuButton.onClick.AddListener(OnBackToMainMenuClicked);

		_startingHP = Player.Health;
		_startingDamage = Player.Damage;
		_startingHPItems = Player.HealItems;
		_startingSpeedItems = Player.SpeedItems;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Return)) {
			GenerateNewFloor();
		}
#endif
	}

	private void OnStartGameClicked()
	{
		UI.IntroScreen.gameObject.SetActive(false);

		Player.Health = _startingHP;
		UI.HPText.text = _startingHP.ToString();

		Player.Damage = _startingDamage;
		UI.DamageText.text = _startingDamage.ToString();

		Player.HealItems = _startingHPItems;
		UI.HealCountText.text = "x " + Player.HealItems;

		Player.SpeedItems = _startingSpeedItems;
		UI.SpeedButtonCount.text = "x " + Player.SpeedItems;

		Player.Coins = 0;
		Player.SetCoins(0);

		CurrentFloor = 0;
		UI.CurrentFloor.text = $"FLOOR: {CurrentFloor.ToString()}";

		Player.CurrentSteps = 0;

		GenerateNewFloor();
	}

	private void OnBackToMainMenuClicked()
	{
		UI.OutroScreen.SetActive(false);

		UI.IntroScreen.gameObject.SetActive(true);
		GenerateNewFloor();
	}

	private void OnPlayerDead()
	{
		Player.IsPlaying = false;

		UI.OutroScreen.SetActive(true);

		UI.OutroScreenCoinsText.text = Player.Coins.ToString();
		UI.OutroScreenFloorText.text = $"FLOOR: {CurrentFloor.ToString()}";
		UI.OutroScreenStepsText.text = $"STEPS: {Player.CurrentSteps}";
	}

	private void GenerateNewFloor()
	{
		if (!Player.IsPlaying) {
			Player.IsPlaying = true;
		}

		Player.StopWalking = true;

		CurrentFloor++;
		UI.CurrentFloor.text = $"FLOOR: {CurrentFloor.ToString()}";

		Board.GenerateLevel(CurrentFloor);
		Player.StartWalking();

		//Player.GivePath(Board.WalkedCells);
	}

	private void OnPlayerReachedExit()
	{
		GenerateNewFloor();
	}

}
