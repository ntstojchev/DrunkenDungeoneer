using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
	public List<Sprite> GroundSprites;
	public List<Sprite> WallSprites;
	public List<Sprite> JunkSprites;

	public Sprite Entrance;
	public Sprite Exit;

	public Sprite Chest;
	public Sprite Web;
	public Sprite Fire;

	public Sprite Heal;
	public Sprite Damage;
	public Sprite Speed;

	public Sprite Enemy;
	public Sprite Obstacle;

	private static TileManager _instance;

	public static TileManager Instance
	{
		get
		{
			if (_instance == null) {
				_instance = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TileManager>();
			}

			return _instance;
		}
	}

	public Sprite GetRandomSprite(List<Sprite> sprites)
	{
		return sprites[Random.Range(0, sprites.Count)];
	}

	public Sprite GetGroundSprite()
	{
		return GetRandomSprite(GroundSprites);
	}

	public Sprite GetWallSprite()
	{
		return GetRandomSprite(WallSprites);
	}

	public Sprite GetJunkSprite()
	{
		return GetRandomSprite(JunkSprites);
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
