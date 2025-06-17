using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private RuleTile ruleTile;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private Tilemap platformTilemap;
    [SerializeField] private TileBase platformTile;

    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private float InitFillPercent = 0.4f;
    [SerializeField] private int softlyCount = 5;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemyPrefabA;
    [SerializeField] private GameObject enemyPrefabB;
    [SerializeField] private int enemyCount = 6;
    [SerializeField] private float minSpawnDistance = 5f;

    [SerializeField] private int platformGap = 5;
    [SerializeField] private int platformMaxLength = 6;

    private bool[,] map;
    private List<Vector2Int> mainRegion;

    void Start()
    {
        CreateMap();
    }

    public void CreateMap()
    {
        wallTilemap.ClearAllTiles();
        platformTilemap.ClearAllTiles();
        GenerateMap();
        mainRegion = GetLargestOpenRegion();
        if (mainRegion == null || mainRegion.Count == 0)
        {
            Debug.LogError("맵 생성 실패: 연결된 빈 공간이 없습니다.");
            return;
        }
        FillNonMainRegion();
        PlaceGroundTiles(mainRegion);
        PlacePlayer(mainRegion);
        SpawnEnemies(mainRegion);
        PlacePlatforms();
    }

    void GenerateMap()
    {
        map = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = Random.value < InitFillPercent;

        for (int i = 0; i < softlyCount; i++)
            SmoothMap();
    }

    void SmoothMap()
    {
        bool[,] newMap = (bool[,])map.Clone();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int walls = GetSurroundWallCount(x, y);
                newMap[x, y] = walls > 4;
            }
        map = newMap;
    }

    int GetSurroundWallCount(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height || (dx != 0 || dy != 0 && map[nx, ny]))
                    count++;
            }
        return count;
    }

    List<Vector2Int> GetLargestOpenRegion()
    {
        bool[,] visited = new bool[width, height];
        List<Vector2Int> largest = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!map[x, y] && !visited[x, y])
                {
                    List<Vector2Int> region = new List<Vector2Int>();
                    Queue<Vector2Int> queue = new Queue<Vector2Int>();
                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        var pos = queue.Dequeue();
                        region.Add(pos);

                        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                        {
                            int nx = pos.x + dir.x;
                            int ny = pos.y + dir.y;

                            if (nx >= 0 && ny >= 0 && nx < width && ny < height && !map[nx, ny] && !visited[nx, ny])
                            {
                                visited[nx, ny] = true;
                                queue.Enqueue(new Vector2Int(nx, ny));
                            }
                        }
                    }

                    if (region.Count > largest.Count)
                        largest = region;
                }
            }
        }

        return largest;
    }

    void FillNonMainRegion()
    {
        HashSet<Vector2Int> keep = new HashSet<Vector2Int>(mainRegion);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (!keep.Contains(new Vector2Int(x, y)))
                    map[x, y] = true;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y])
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), ruleTile);
    }

    void PlaceGroundTiles(List<Vector2Int> region)
    {
        foreach (var pos in region)
        {
            int x = pos.x, y = pos.y;
            if (x + 4 < width)
            {
                bool valid = true;
                for (int dx = 0; dx < 4; dx++)
                    if (map[x + dx, y])
                        valid = false;
                if (valid)
                {
                    for (int dx = 0; dx < 4; dx++)
                        wallTilemap.SetTile(new Vector3Int(x + dx, y, 0), groundTile);
                }
            }
        }
    }

    void PlacePlayer(List<Vector2Int> region)
    {
        foreach (var pos in region)
        {
            int x = pos.x, y = pos.y;
            if (y > 1 && y < height - 1 && !map[x, y - 1] && !map[x, y + 1])
            {
                Vector3 worldPos = wallTilemap.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, 0.5f);
                player.transform.position = worldPos;
                return;
            }
        }
    }

    void SpawnEnemies(List<Vector2Int> region)
    {
        int countA = enemyCount / 2 + (enemyCount % 2), countB = enemyCount / 2, tries = 1000;
        int spawnedA = 0, spawnedB = 0;

        while ((spawnedA < countA || spawnedB < countB) && tries-- > 0)
        {
            Vector2Int pos = region[Random.Range(0, region.Count)];
            Vector3 worldPos = wallTilemap.CellToWorld(new Vector3Int(pos.x, pos.y, 0)) + new Vector3(0.5f, 0.5f);

            if (Vector2.Distance(worldPos, player.transform.position) < minSpawnDistance) continue;

            if (spawnedA < countA)
            {
                Instantiate(enemyPrefabA, worldPos, Quaternion.identity);
                spawnedA++;
            }
            else if (spawnedB < countB)
            {
                Instantiate(enemyPrefabB, worldPos, Quaternion.identity);
                spawnedB++;
            }
        }
    }

    void PlacePlatforms()
    {
        for (int y = 0; y < height; y += platformGap)
        {
            int len = 0;
            for (int x = 0; x < width; x++)
            {
                if (!map[x, y])
                {
                    platformTilemap.SetTile(new Vector3Int(x, y, 0), platformTile);
                    len++;
                    if (len >= platformMaxLength)
                    {
                        x += 2;
                        len = 0;
                    }
                }
                else
                {
                    len = 0;
                }
            }
        }
    }
}