using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
	public Cell CellPrefab;
	public GameObject Root;
	public int UpperLimit = 20;
	public int Rows = 25;
	public int Columns = 25;

	public int ChestChance = 30;
	public int FireChance = 15;
	public int WebChance = 15;
	public int HealChance = 10;
	public int DamageChance = 10;
	public int SpeedChance = 10;

	public int EnemyChance = 5;
	public int ObstacleChance = 5;

	[Range(0, 1)]
	public float MaxWalkedCells = 0.8f;
	public bool GenerateInstant;

	public List<Cell> WalkedCells = new List<Cell>();
	public List<Cell> InteractiveCells = new List<Cell>();
	public Cell Entrance;
	public Cell Exit;

	public Cell[,] Cells;

	private int _cellCount => Rows * Columns;
	private int _fillCells = 0;

	private int _currentWalkedCells = 0;

	private Cell _currentCell;
	private Cell[] orthCells;

	private int _floor;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

	public void GenerateLevel(int floor)
	{
		_floor = floor;

		GenerateDungeon();

		UnleashTheDrunkard();
	}

	private void GenerateDungeon()
	{
		foreach (Transform child in Root.transform) {
			Destroy(child.gameObject);
		}

		int size = UnityEngine.Random.Range(12, UpperLimit+1);

		Rows = size;
		Columns = size;

		Cells = new Cell[Rows, Columns];
		_fillCells = Mathf.FloorToInt((Rows * Columns) * MaxWalkedCells);

		for (int row = 0; row < Rows; row++) {
			for (int column = 0; column < Columns; column++) {
				Cell cell = Instantiate(CellPrefab, Root.transform);
				cell.name = $"Cell x:{row} y:{column}";
				cell.transform.position = new Vector3(row, column, 0);

				cell.Row = row;
				cell.Column = column;
				cell.SetWall();

				Cells[row, column] = cell;
			}
		}
	}

	private void UnleashTheDrunkard()
	{
		_currentWalkedCells = 0;
		_currentCell = null;

		StopAllCoroutines();
		StartCoroutine(Walk());
	}

	private Cell GetRandomCell()
	{
		return Cells[UnityEngine.Random.Range(1, Rows-1), UnityEngine.Random.Range(1, Columns-1)];
	}

	private Cell GetRandomCellFrom(Cell[] cells)
	{
		return cells[UnityEngine.Random.Range(0, cells.Length)];
	}

	public  Cell[] GetCellsInOrthogonalDirections(Cell cell)
	{
		var temp = new List<Cell>();

		temp.Add(GetCellFrom(cell.Row, cell.Column + 1));
		temp.Add(GetCellFrom(cell.Row, cell.Column - 1));
		temp.Add(GetCellFrom(cell.Row - 1, cell.Column));
		temp.Add(GetCellFrom(cell.Row + 1, cell.Column));

		return temp.Where(c => c != null).ToArray();
	}

	private Cell[] GetCellsInDiagonalDirections(Cell cell)
	{
		var temp = new List<Cell>();

		temp.Add(GetCellFrom(cell.Row - 1, cell.Column + 1));
		temp.Add(GetCellFrom(cell.Row + 1, cell.Column - 1));
		temp.Add(GetCellFrom(cell.Row - 1, cell.Column - 1));
		temp.Add(GetCellFrom(cell.Row + 1, cell.Column + 1));

		return temp.Where(c => c != null).ToArray();
	}

	private Cell GetCellFrom(int x, int y)
	{
		if (x < 1 || x >= Rows - 1) {
			return null;
		}

		if (y < 1 || y >= Columns - 1) {
			return null;
		}

		return Cells[x, y];
	}

	private IEnumerator Walk()
	{
		WalkedCells.Clear();

		if (_currentCell == null) {
			_currentCell = GetRandomCell();
			_currentCell.SetEmpty();

			WalkedCells.Add(_currentCell);
		}

		while (_currentWalkedCells < _fillCells) {
			orthCells = GetCellsInOrthogonalDirections(_currentCell);
			_currentCell = GetRandomCellFrom(orthCells);

			if (_currentCell.CurrentCellState == Cell.CellState.Wall) {
				_currentCell.SetEmpty();

				_currentWalkedCells++;
			}

			if (!WalkedCells.Contains(_currentCell))
				WalkedCells.Add(_currentCell);

			if (!GenerateInstant)
				yield return new WaitForSeconds(0.001f);
		}

		PolishDungeon();
		FloodFillFloor();
	}

	private void PolishDungeon()
	{
		var cells = new List<Cell>();
		for (int row = 0; row < Rows; row++) {
			for (int column = 0; column < Columns; column++) {
				cells.Clear();
				cells.AddRange(GetCellsInOrthogonalDirections(Cells[row, column]));
				cells.AddRange(GetCellsInDiagonalDirections(Cells[row, column]));

				bool voidTile = true;
				foreach (Cell cell in cells) {
					if (cell.CurrentCellState == Cell.CellState.Empty) {
						voidTile = false;
					}
				}

				if (voidTile) {
					Cells[row, column].SetVoid();
				}
			}
		}

		Entrance = WalkedCells.First();
		Entrance.SetEntranceCell();

		Exit = WalkedCells[UnityEngine.Random.Range(WalkedCells.Count/2, WalkedCells.Count)];
		Exit.SetExitCell();

		FloodFillFloor();

		GenerateInteractivity();
	}

	private int _currentDistance;
	private void FloodFillFloor()
	{
		_currentDistance = 0;
		Exit.Distance = _currentDistance;

		orthCells = GetCellsInOrthogonalDirections(Exit);
		foreach (Cell cell in orthCells) {
			cell.SetDistance(_currentDistance + 1);
		}

		while (CheckForFloodFill()) {
			ContinueFlood();
			_currentDistance++;
		}
	}

	private void ContinueFlood()
	{
		var cells = new List<Cell>();
		for (int row = 0; row < Rows; row++) {
			for (int column = 0; column < Columns; column++) {
				if (Cells[row, column].CurrentCellState == Cell.CellState.Empty) {
					if (Cells[row, column].Distance == _currentDistance) {
						cells.Add(Cells[row, column]);
					}
				}
			}
		}

		foreach (Cell cell in cells) {
			orthCells = GetCellsInOrthogonalDirections(cell);
			foreach (Cell orthCell in orthCells) {
				if (orthCell.CurrentCellState == Cell.CellState.Empty && orthCell.Distance == -1) {
					orthCell.SetDistance(_currentDistance + 1);
				}
			}
		}
	}

	private bool CheckForFloodFill()
	{
		foreach (Cell cell in WalkedCells) {
			if (cell.Distance == -1) {
				return true;
			}
		}

		return false;
	}

	private void GenerateInteractivity()
	{
		InteractiveCells = new List<Cell>(WalkedCells);

		GenerateChests();
		GenerateFire();
		GenerateWeb();
		GenerateHeal();
		GenerateSpeed();
		GenerateDamage();
		GenerateEnemy();
		GenerateObstacle();
	}

	private void GenerateChests()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach(Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < ChestChance) {
				cell.SetChest();
			}
		}
	}

	private void GenerateFire()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach (Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < FireChance) {
				cell.SetFire();
			}
		}
	}

	private void GenerateWeb()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach (Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < WebChance) {
				cell.SetWeb();
			}
		}
	}

	private void GenerateHeal()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach (Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < Mathf.Clamp(HealChance - _floor, 5, HealChance)) {
				cell.SetHeal();
			}
		}
	}

	private void GenerateSpeed()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach (Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < Mathf.Clamp(SpeedChance - _floor, 5, SpeedChance)) {
				cell.SetSpeed();
			}
		}
	}

	private void GenerateDamage()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach (Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < DamageChance) {
				cell.SetDamage();
			}
		}
	}

	private void GenerateEnemy()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach (Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < Mathf.Clamp(EnemyChance + _floor, EnemyChance, 13)) {
				cell.SetEnemy();
			}
		}
	}

	private void GenerateObstacle()
	{
		IEnumerable<Cell> cells = InteractiveCells.Where(c => c.InteractiveState == Cell.CellInteractiveState.None);

		foreach (Cell cell in cells) {
			if (UnityEngine.Random.Range(0, 100) < Mathf.Clamp(ObstacleChance + _floor, ObstacleChance, 10)) {
				cell.SetObstacle();
			}
		}
	}
}
