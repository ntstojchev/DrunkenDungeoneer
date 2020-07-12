using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
	public enum CellState
	{
		Empty,
		Wall,
	}

	public enum CellInteractiveState
	{
		None,
		Entrance,
		Exit,
		Web,
		Fire,
		Chest,
		Heal,
		Damage,
		Speed,
		Enemy,
		EnemyObstacle,
	}

	public SpriteRenderer Background;
	public SpriteRenderer Foreground;
	public TextMesh DistanceText;

	public int Row;
	public int Column;
	public int JunkChance = 25;
	public int Distance = -1;

	public CellState CurrentCellState;
	public CellInteractiveState InteractiveState;

	public bool IsEntrance => InteractiveState == CellInteractiveState.Entrance;
	public bool IsExit => InteractiveState == CellInteractiveState.Exit;
	public bool IsNone => InteractiveState == CellInteractiveState.None;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ClearInteractive()
	{
		SetInteractiveState(CellInteractiveState.None);
		Foreground.gameObject.SetActive(false);
	}

	public void SetWall()
	{
		SetState(CellState.Wall);

		Foreground.gameObject.SetActive(false);
		Background.color = Color.white;

		Background.sprite = TileManager.Instance.GetWallSprite();
	}

	public void SetEmpty()
	{
		SetState(CellState.Empty);

		Foreground.gameObject.SetActive(true);
		Background.color = Color.white;

		Background.sprite = TileManager.Instance.GetGroundSprite();

		if (Random.Range(0, 101) < JunkChance) {
			Foreground.sprite = TileManager.Instance.GetJunkSprite();
		}
	}

	public void SetVoid()
	{
		Background.gameObject.SetActive(false);
		Foreground.gameObject.SetActive(false);
	}

	public void SetEntranceCell()
	{
		SetInteractiveState(CellInteractiveState.Entrance);
		Foreground.sprite = TileManager.Instance.Entrance;
	}

	public void SetExitCell()
	{
		SetInteractiveState(CellInteractiveState.Exit);
		Foreground.sprite = TileManager.Instance.Exit;
	}

	public void SetChest()
	{
		InteractiveState = CellInteractiveState.Chest;
		Foreground.sprite = TileManager.Instance.Chest;
	}

	public void SetWeb()
	{
		InteractiveState = CellInteractiveState.Web;
		Foreground.sprite = TileManager.Instance.Web;
	}

	public void SetFire()
	{
		InteractiveState = CellInteractiveState.Fire;
		Foreground.sprite = TileManager.Instance.Fire;
	}

	public void SetHeal()
	{
		InteractiveState = CellInteractiveState.Heal;
		Foreground.sprite = TileManager.Instance.Heal;
	}

	public void SetDamage()
	{
		InteractiveState = CellInteractiveState.Damage;
		Foreground.sprite = TileManager.Instance.Damage;
	}

	public void SetSpeed()
	{
		InteractiveState = CellInteractiveState.Speed;
		Foreground.sprite = TileManager.Instance.Speed;
	}

	public void SetEnemy()
	{
		InteractiveState = CellInteractiveState.Enemy;
		Foreground.sprite = TileManager.Instance.Enemy;
	}

	public void SetObstacle()
	{
		InteractiveState = CellInteractiveState.EnemyObstacle;
		Foreground.sprite = TileManager.Instance.Obstacle;
	}

	public void SetDistance(int distance)
	{
		Distance = distance;
		DistanceText.text = distance.ToString();
	}

	private void SetState(CellState state)
	{
		CurrentCellState = state;
	}

	private void SetInteractiveState(CellInteractiveState state)
	{
		InteractiveState = state;
	}
}
