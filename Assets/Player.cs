using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
	public Action ReachedExit;
	public Action DrunkardDead;

	public UIManager UI;
	public BoardManager Board;

	public bool IsPlaying = false;
	public float LowSpeed;
	public float NormalSpeed;
	public float HighSpeed;
	public float SpeedBuffTime = 3f;

	public int Health = 5;
	public int Damage = 5;
	public int Coins = 0;

	public int HealItems = 1;
	public float SoberCooldown = 5f;
	public float SoberDuration = 2f;
	public int SpeedItems = 1;

	public SpriteRenderer Visual;

	public float Speed = 0.5f;
	public bool StopWalking;
	public bool SnapToRoute = false;

	public int CurrentSteps;
	public int MaxStep;

	private List<Cell> _walkable;
	private bool _speedChanged;
	private float _speedBuffStartTime;

	private bool _soberUp;
	private float _soberUpStartTime;
	private float _soberUpFinishTime;

    // Start is called before the first frame update
    void Start()
    {
		UI.HealButton.onClick.AddListener(ConsumeHeal);
		UI.SoberButton.onClick.AddListener(SoberUp);
		UI.SpeedButton.onClick.AddListener(ConsumeSpeed);

		UI.HealCountText.text = "x " + HealItems;
		UI.SpeedButtonCount.text = "x " + SpeedItems;

		UI.HPText.text = Health.ToString();
		UI.DamageText.text = Damage.ToString();

		UI.SoberCooldownDuration.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
    {
		if (IsPlaying) {
			if (_speedChanged) {
				UI.SpeedBuffTimeText.gameObject.SetActive(true);

				if (Time.time - _speedBuffStartTime > SpeedBuffTime) {
					_speedChanged = false;

					UI.SpeedBuffTimeText.gameObject.SetActive(false);

					SetSpeed(NormalSpeed);
				}
				else {
					UI.SpeedBuffTimeText.text = (SpeedBuffTime - (Time.time - _speedBuffStartTime)).ToString("#0.00");
				}
			}

			if (_soberUp) {
				UI.SoberCooldownDuration.gameObject.SetActive(true);
				SnapToRoute = true;

				if (Time.time - _soberUpStartTime > SoberDuration) {
					_soberUp = false;
					_soberUpFinishTime = Time.time;

					UI.SoberCooldownDuration.gameObject.SetActive(false);

					SnapToRoute = false;
				}
				else {
					UI.SoberCooldownDuration.text = (SoberDuration - (Time.time - _soberUpStartTime)).ToString("#0.00");
				}
			}
			else {
				if (Time.time - _soberUpFinishTime > SoberCooldown) {
					UI.SoberButton.interactable = true;

					UI.SoberCooldownDuration.gameObject.SetActive(false);
				}
				else {
					UI.SoberButton.interactable = false;

					UI.SoberCooldownDuration.gameObject.SetActive(true);
					UI.SoberCooldownDuration.text = (SoberCooldown - (Time.time - _soberUpFinishTime)).ToString("#0.00");
				}
			}
		}
	}

	public void GivePath(List<Cell> cells)
	{
		_walkable = cells;
		MaxStep = _walkable.Count;

		StopWalking = false;

		StopAllCoroutines();
		StartCoroutine(Walk());
	}

	private Cell _currentCell;
	private Cell _lastCell;

	public void StartWalking()
	{
		_currentCell = Board.Entrance;

		StopAllCoroutines();
		StartCoroutine(Walk());
	}

	private IEnumerator Walk()
	{
		while (_currentCell.IsExit == false) {

			if (Health <= 0) {
				break;
			}

			List<Cell> possibleWalks = new List<Cell>();
			if (SnapToRoute) {
				StepOnRoute(possibleWalks);
			}
			else {
				Cell[] cells = Board.GetCellsInOrthogonalDirections(_currentCell);
				possibleWalks = cells.Where(c => c.CurrentCellState == Cell.CellState.Empty).ToList();

				if (possibleWalks.Count > 1 && possibleWalks.Contains(_lastCell)) {
					possibleWalks.Remove(_lastCell);
				}
			}

			Cell nextCell = possibleWalks[UnityEngine.Random.Range(0, possibleWalks.Count)];

			_lastCell = _currentCell;
			_currentCell = nextCell;

			transform.position = _currentCell.transform.position;

			CurrentSteps++;
			UI.WalkableText.text = $"STEPS: {CurrentSteps}";

			CheckForInteractivity();

			yield return new WaitForSeconds(Speed);
		}

		if (_currentCell.IsExit) {
			ReachedExit?.Invoke();
		}
	}

	private void CheckForInteractivity()
	{
		if (_currentCell.InteractiveState == Cell.CellInteractiveState.Chest) {
			_currentCell.ClearInteractive();

			SetCoins(UnityEngine.Random.Range(0, 100));
		}
		else if (_currentCell.InteractiveState == Cell.CellInteractiveState.Web) {
			SetSpeed(LowSpeed);

			_speedChanged = true;
			_speedBuffStartTime = Time.time;
		}
		else if (_currentCell.InteractiveState == Cell.CellInteractiveState.Fire) {
			SetSpeed(HighSpeed);

			_speedChanged = true;
			_speedBuffStartTime = Time.time;
		}
		else if (_currentCell.InteractiveState == Cell.CellInteractiveState.Heal) {
			_currentCell.ClearInteractive();

			HealItems++;
			UI.HealCountText.text = "x " + HealItems;

			if (HealItems > 0) {
				UI.HealButton.interactable = true;
			}
		}
		else if (_currentCell.InteractiveState == Cell.CellInteractiveState.Damage) {
			_currentCell.ClearInteractive();

			Damage++;
			UI.DamageText.text = Damage.ToString();
		}
		else if (_currentCell.InteractiveState == Cell.CellInteractiveState.Speed) {
			_currentCell.ClearInteractive();

			SpeedItems++;
			UI.SpeedButtonCount.text = "x " + SpeedItems;

			if (SpeedItems > 0) {
				UI.SpeedButton.interactable = true;
			}
		}
		else if (_currentCell.InteractiveState == Cell.CellInteractiveState.Enemy) {
			_currentCell.ClearInteractive();

			if (Damage > 0) {
				Damage--;
				UI.DamageText.text = Damage.ToString();
			}
			else {
				Health--;
				UI.HPText.text = Health.ToString();

				if (Health <= 0) {
					StopAllCoroutines();
					DrunkardDead?.Invoke();
				}
			}
		}
		else if (_currentCell.InteractiveState == Cell.CellInteractiveState.EnemyObstacle) {
			Health--;
			UI.HPText.text = Health.ToString();

			if (Health <= 0) {
				DrunkardDead?.Invoke();
			}
		}
	}

	public void SetSpeed(float speed)
	{
		if (speed == LowSpeed) {
			UI.SpeedText.text = "SLOW";
			Speed = LowSpeed;
		}
		else if (speed == NormalSpeed) {
			UI.SpeedText.text = "NORMAL";

			Speed = NormalSpeed;
		}
		else if (speed == HighSpeed) {
			UI.SpeedText.text = "FAST";
			Speed = HighSpeed;
		}
	}

	public void SetCoins(int coins)
	{
		Coins += coins;
		UI.Coins.text = Coins.ToString();
	}

	private void ConsumeHeal()
	{
		HealItems--;
		UI.HealCountText.text = "x " + HealItems;

		if (HealItems <= 0) {
			UI.HealButton.interactable = false;
		}

		Health += UnityEngine.Random.Range(1, 4);
		UI.HPText.text = Health.ToString();
	}

	private void ConsumeSpeed()
	{
		SpeedItems--;
		UI.SpeedButtonCount.text = "x " + SpeedItems;

		if (SpeedItems <= 0) {
			UI.SpeedButton.interactable = false;
		}

		_speedChanged = true;
		_speedBuffStartTime = Time.time;

		SetSpeed(HighSpeed);
	}

	private void SoberUp()
	{
		_soberUp = true;
		_soberUpStartTime = Time.time;

		UI.SoberButton.interactable = false;
	}

	private void StepOnRoute(List<Cell> possibleWalks)
	{
		Cell[] cells = Board.GetCellsInOrthogonalDirections(_currentCell);

		foreach (Cell cell in cells) {
			if (cell.Distance == -1) {
				continue;
			}

			if (cell.Distance < _currentCell.Distance) {
				possibleWalks.Add(cell);
			}
		}

		if (possibleWalks.Count == 0) {
			foreach (Cell cell in cells) {
				if (cell.Distance >= _currentCell.Distance) {
					possibleWalks.Add(cell);
				}
			}
		}
	}

	//private IEnumerator Walk()
	//{
	//	CurrentStep = 0;

	//	foreach (Cell cell in _walkable) {
	//		if (StopWalking) {
	//			break;
	//		}

	//		CurrentStep++;
	//		UI.WalkableText.text = $"{CurrentStep}/{MaxStep}";

	//		transform.position = cell.transform.position;

	//		if (cell.IsExit) {
	//			ReachedExit?.Invoke();
	//			break;
	//		}

	//		yield return new WaitForSeconds(Speed);
	//	}
	//}
}
