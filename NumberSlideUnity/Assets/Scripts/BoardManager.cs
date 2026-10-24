using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class BoardSkin
    {
        public Sprite Board;
        public GameObject TilePrefab;
    }

    [SerializeField] private BoardSkin[] BoardSkins;

    private Sprite CurrentBoard;
    private GameObject CurrentTilePrefab;
    private float[][][] CurrentTilePosition;
    private BoardSkin CurrentBoardSkin;
    private List<GameObject> Tiles = new();
    private byte[,] Map;
    private byte[,] OldMap;
    
    // Start is called before the first frame update
    void Awake()
    {
        CurrentBoardSkin = BoardSkins[Session.Difficulty - 3];
        CurrentBoard = CurrentBoardSkin.Board;
        CurrentTilePrefab = CurrentBoardSkin.TilePrefab;

        string layoutPath = $"BoardLayouts/{Session.Difficulty}x{Session.Difficulty}";
        TextAsset layoutAsset = Resources.Load<TextAsset>(layoutPath);
        if (layoutAsset == null)
        {
            throw new InvalidOperationException($"找不到棋盘布局资源：{layoutPath}");
        }

        CurrentTilePosition =
            JsonSerializer.Deserialize<float[][][]>(layoutAsset.text)
            ?? throw new InvalidOperationException($"无法解析棋盘布局：{layoutPath}");

        transform.position = new Vector3(0, 0);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = CurrentBoard;
    }

    void Start()
    {
        DeepCopy(GameManager.Instance.game.board.BoardMap, out Map);
        DeepCopy(Map, out OldMap);
        ShowMap();
    }
    
    // Update is called once per frame
    void Update()
    {
        DeepCopy(GameManager.Instance.game.board.BoardMap, out Map);
        if (Session.Moved)
        {
            ShowMap();
            DeepCopy(Map, out OldMap);
            Session.Moved = false;
        }
    }

    void ShowMap()
    {
        foreach (var tile in Tiles) Destroy(tile);
        Tiles.Clear();

        for (int i = 0; i < Map.GetLength(0); i++)
        {
            for (int j = 0; j < Map.GetLength(1); j++)
            {
                if (Map[i, j] == 0) continue;
                
                float[] position = CurrentTilePosition[i][j];
                Vector3 posVect = new Vector3(position[0], position[1]);

                GameObject tile = Instantiate(CurrentTilePrefab, transform);
                tile.transform.localPosition = posVect;
                TextMeshPro tmp = tile.GetComponentInChildren<TextMeshPro>();
                tmp.text = Map[i, j].ToString();
                Tiles.Add(tile);
            }
        }
    }

    void DeepCopy(byte[,] source, out byte[,] destination)
    {
        destination = new byte[source.GetLength(0), source.GetLength(1)];

        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int j = 0; j < source.GetLength(1); j++)
            {
                destination[i, j] = source[i, j];
            }
        }
    }

    bool ArrayEqual(byte[,] array1, byte[,] array2)
    {
        if (array1.GetLength(0) != array2.GetLength(0) || array1.GetLength(1) != array2.GetLength(1))
            return false;

        int xLength = array1.GetLength(0);
        int yLength = array1.GetLength(1);

        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            {
                if (array1[x, y] != array2[x, y]) return false;
            }
        }

        return true;
    }
}
