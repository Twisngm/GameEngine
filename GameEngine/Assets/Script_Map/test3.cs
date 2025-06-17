using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellularAutomata2 : MonoBehaviour
{
    [Header("Tilemap �� Ÿ�� ����")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private RuleTile ruleTile;         // �� Ÿ��: Rule Tile
    [SerializeField] private TileBase groundTile;       // �ٴ� �߾� ��
    [SerializeField] private GameObject playerSpawnPrefab;

    [Header("�� ����")]
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private float InitFillPercent = 0.45f;
    [SerializeField] private int softlyCount = 5;

    [Header("�߾� ���� ����")]
    [SerializeField] private int MinArea = 4;
    [SerializeField] private int MaxWidthLimit = 8; // = MinArea + 4

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
        CreatePlayerSpawnPoint();
    }

    void GenerateMap()
    {
        map = new bool[width, height];
        RandomFillMap();

        for (int i = 0; i < softlyCount; i++)
        {
            SmoothMap();
        }
        EnsureBorders();
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
                if (neighborWallCount > 4) map[x, y] = true;
                else if (neighborWallCount < 4) map[x, y] = false;
            }
        }
    }

    void EnsureBorders()
    {
        for (int x = 0; x < width; x++)
        {
            map[x, 0] = true;
            map[x, height - 1] = true;
        }

        for (int y = 0; y < height; y++)
        {
            map[0, y] = true;
            map[width - 1, y] = true;
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
                        wallCount += map[neighborX, neighborY] ? 1 : 0;
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
        // �� Ÿ�� ��ġ
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y])
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), ruleTile);
                }
            }
        }

        // �߾� groundTile ��ġ
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (visited[x, y] || map[x, y]) continue;

                int maxWidth = 0;
                while (x + maxWidth < width && !map[x + maxWidth, y] && !visited[x + maxWidth, y])
                    maxWidth++;

                int maxHeight = 0;
                while (true)
                {
                    if (y + maxHeight >= height) break;
                    bool rowValid = true;
                    for (int i = 0; i < maxWidth; i++)
                    {
                        if (map[x + i, y + maxHeight] || visited[x + i, y + maxHeight])
                        {
                            rowValid = false;
                            break;
                        }
                    }
                    if (!rowValid) break;
                    maxHeight++;
                }

                if (maxWidth >= MinArea && maxHeight >= MinArea)
                {
                    int clampedWidth = Mathf.Min(maxWidth, MaxWidthLimit);
                    int targetRow = y + MinArea / 2;

                    if (targetRow < y + maxHeight && targetRow < height)
                    {
                        for (int dx = 0; dx < clampedWidth; dx++)
                        {
                            Vector3Int pos = new Vector3Int(x + dx, targetRow, 0);
                            tilemap.SetTile(pos, groundTile);
                            visited[x + dx, targetRow] = true;
                        }
                    }
                }
            }
        }
    }

    void CreatePlayerSpawnPoint()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyStartY = -1;
            int emptyCount = 0;

            for (int y = 0; y < height; y++)
            {
                if (!map[x, y])
                {
                    if (emptyStartY == -1)
                        emptyStartY = y;
                    emptyCount++;
                }
                else
                {
                    if (emptyCount >= 3)
                        break;
                    emptyStartY = -1;
                    emptyCount = 0;
                }
            }

            if (emptyCount >= 3)
            {
                int spawnY = emptyStartY + emptyCount / 2;
                Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(x, spawnY, 0)) + new Vector3(0.5f, 0.5f, 0f);
                Instantiate(playerSpawnPrefab, worldPos, Quaternion.identity);
                Debug.Log($"�÷��̾� ���� ��ġ: ({x}, {spawnY})");
                return;
            }
        }

        Debug.LogWarning("������ �÷��̾� ���� ��ġ�� ã�� ���߽��ϴ�.");
    }
}
