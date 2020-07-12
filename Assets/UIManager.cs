using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public BoardManager Board;
	public Player Player;

	public Button StartGameButton;
	public Button BackToMainMenuButton;
	public GameObject IntroScreen;

	public GameObject OutroScreen;
	public Text OutroScreenStepsText;
	public Text OutroScreenCoinsText;
	public Text OutroScreenFloorText;

	public Text WalkableText;
	public Button ShowDistances;
	public Button SnapToRoute;
	public Text SpeedText;
	public Text SpeedBuffTimeText;
	public Text Coins;
	public Text CurrentFloor;
	public Text HPText;
	public Text DamageText;

	public Button SoberButton;
	public Text SoberCooldownDuration;

	public Button SpeedButton;
	public Text SpeedButtonCount;

	public Button HealButton;
	public Text HealCountText;

	private bool _showDistances;

    // Start is called before the first frame update
    void Start()
    {
		ShowDistances.onClick.AddListener(OnShowDistances);
		SnapToRoute.onClick.AddListener(OnSnapToRoute);
	}

	// Update is called once per frame
	void Update()
    {
        
    }

	private void OnShowDistances()
	{
		_showDistances = !_showDistances;
		foreach (Cell cell in Board.Cells) {
			cell.DistanceText.gameObject.SetActive(_showDistances);
		}
	}

	private void OnSnapToRoute()
	{
		Player.SnapToRoute = !Player.SnapToRoute;
	}
}
