using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase groundTile;
    [SerializeField] TileBase wallTile;
    [SerializeField] TileBase borderTile;
    [SerializeField] int width = 50;
    [SerializeField] int height = 50;
    [SerializeField] float InitFillPercent = 0.45f;
    [SerializeField] int softlyCount = 5;

    [SerializeField] int JumpValue = 3;
    [SerializeField] int Tilewidth = 4;

    private bool[,] map;

    private void Start()
    {
        CreateMap();
    }
    public void CreateMap()
    {
        tilemap.ClearAllTiles();
        GenerateMap();
        Placement_tile();
    }

    void GenerateMap()
    {
        map = new bool[width, height];
        RandomFillMap();

        for (int i = 0; i < softlyCount; i++)
        {
            SmoothMap();
        }
    }

    void RandomFillMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (Random.value < InitFillPercent);
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighborWallCount = GetSurroundWallCount(x, y);

                if (neighborWallCount > 4)
                {
                    map[x, y] = true;
                }
                else if (neighborWallCount < 4)
                {
                    map[x, y] = false;
                }
            }
        }
    }

    int GetSurroundWallCount(int gridX, int gridY)
    {
        int wallCount = 0;

        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        wallCount += (map[neighborX, neighborY]) ? 1 : 0;
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    void Placement_tile()
    {
        for (int i = -1; i < width + 1; i++)
        {
            Vector3Int leftPos = new Vector3Int(i, -1, 0);
            tilemap.SetTile(leftPos, borderTile);
            Vector3Int rightpos = new Vector3Int(i, height, 0);
            tilemap.SetTile(rightpos, borderTile);
        }
        for (int i = -1; i < height + 1; i++)
        {
            Vector3Int leftPos = new Vector3Int(-1, i, 0);
            tilemap.SetTile(leftPos, borderTile);
            Vector3Int rightpos = new Vector3Int(width, i, 0);
            tilemap.SetTile(rightpos, borderTile);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (map[x, y])
                    tilemap.SetTile(cellPosition, wallTile);
                
            }
        }
    }
}