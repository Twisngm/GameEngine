using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellularAutomata2 : MonoBehaviour
{
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap platformTilemap;
    [SerializeField] private RuleTile ruleTile;
    [SerializeField] private TileBase platformTile;
    [SerializeField] private GameObject player;
    [SerializeField] private int maxPlatformLength = 4;

    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private float InitFillPercent = 0.45f;
    [SerializeField] private int softlyCount = 5;

    [SerializeField] GameObject enemyPrefabGoblin;
    [SerializeField] GameObject enemyPrefabSkeleton;
    [SerializeField] int enemyCount = 5;
    [SerializeField] float minSpawnDistance = 5f;

    private List<Vector2Int> mainRegion;
    private Transform playerTransform;


    private bool[,] map;
    private const float MaxOpenAreaRatio = 0.35f;
    private int MaxFloodSize => (int)(width * height * MaxOpenAreaRatio);

    private void Start()
    {
        CreateMap();
        
    }

    public void CreateMap()
    {
        wallTilemap.ClearAllTiles();
        platformTilemap.ClearAllTiles();
        GenerateMap();
        EnsureBorders();
        ApplyWallTilemap();
        var mainRegion = RemoveDisconnectedRegions();
        CreatePlayerSpawn(mainRegion);
        GeneratePlatforms(mainRegion);
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

    int GetSurroundWallCount(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    if (dx != 0 || dy != 0)
                        count += map[nx, ny] ? 1 : 0;
                }
                else count++;
            }
        }
        return count;
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

    void ApplyWallTilemap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y])
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), ruleTile);
    }

    List<Vector2Int> FloodFill(int startX, int startY, bool[,] visited)
    {
        List<Vector2Int> region = new();
        Queue<Vector2Int> queue = new();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            region.Add(cur);

            if (region.Count > MaxFloodSize)
                return new List<Vector2Int>();

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                int nx = cur.x + dir.x;
                int ny = cur.y + dir.y;
                if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 &&
                    !map[nx, ny] && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return region;
    }

    List<Vector2Int> RemoveDisconnectedRegions()
    {
        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> regions = new();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (!map[x, y] && !visited[x, y])
                {
                    var region = FloodFill(x, y, visited);
                    if (region.Count > 0)
                        regions.Add(region);
                }
            }
        }

        if (regions.Count == 0) return new();

        regions.Sort((a, b) => b.Count.CompareTo(a.Count));
        var mainRegion = regions[0];

        if (mainRegion.Count > MaxFloodSize)
        {
            Debug.LogWarning("Main region too large ? possibly invalid map. Cancelling.");
            return new();
        }

        for (int i = 1; i < regions.Count; i++)
            foreach (var p in regions[i])
            {
                map[p.x, p.y] = true;
                wallTilemap.SetTile(new Vector3Int(p.x, p.y, 0), ruleTile);
            }

        return mainRegion;
    }

    void CreatePlayerSpawn(List<Vector2Int> region)
    {
        HashSet<Vector2Int> regionSet = new(region);

        for (int x = 0; x < width; x++)
        {
            for (int y = 2; y < height - 2; y++) // 위아래 2칸 확보
            {
                Vector2Int pos = new(x, y);
                if (!map[x, y] &&
                    !map[x, y + 1] && !map[x, y + 2] &&
                    !map[x, y - 1] && !map[x, y - 2] &&
                    regionSet.Contains(pos))
                {
                    Vector3 worldPos = wallTilemap.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, 0.5f, 0);
                    player.transform.position = worldPos;
                    return;
                }
            }
        }

        Debug.LogWarning("No suitable player spawn point found.");
    }

    void SpawnEnemies(List<Vector2Int> region)
    {
        int countA = enemyCount / 2 + (enemyCount % 2); // 홀수일 경우 A가 하나 더
        int countB = enemyCount / 2;

        int spawnedA = 0;
        int spawnedB = 0;
        int maxTries = 1000;

        while ((spawnedA < countA || spawnedB < countB) && maxTries-- > 0)
        {
            Vector2Int randomPos = region[Random.Range(0, region.Count)];
            Vector3 worldPos = wallTilemap.CellToWorld(new Vector3Int(randomPos.x, randomPos.y, 0)) + new Vector3(0.5f, 0.5f);

            if (Vector2.Distance(worldPos, playerTransform.position) < minSpawnDistance)
                continue;

            if (spawnedA < countA)
            {
                Instantiate(enemyPrefabGoblin, worldPos, Quaternion.identity);
                spawnedA++;
            }
            else if (spawnedB < countB)
            {
                Instantiate(enemyPrefabSkeleton, worldPos, Quaternion.identity);
                spawnedB++;
            }
        }

        if (spawnedA + spawnedB < enemyCount)
            Debug.LogWarning($"Spawned only {spawnedA + spawnedB}/{enemyCount} enemies (not enough space far from player)");
    }


    void GeneratePlatforms(List<Vector2Int> region)
    {
        HashSet<Vector2Int> regionSet = new(region);

        for (int y = 0; y < height; y++)
        {
            if (y % 5 != 0) continue;

            int length = 0;
            int startX = -1;

            for (int x = 0; x < width; x++)
            {
                Vector2Int cur = new(x, y);

                if (regionSet.Contains(cur)) // ← 조건 완화됨
                {
                    if (length == 0) startX = x;
                    length++;

                    if (length >= maxPlatformLength)
                    {
                        for (int i = 0; i < length; i++)
                            platformTilemap.SetTile(new Vector3Int(startX + i, y, 0), platformTile);
                        length = 0;
                    }
                }
                else if (length > 0)
                {
                    for (int i = 0; i < length; i++)
                        platformTilemap.SetTile(new Vector3Int(startX + i, y, 0), platformTile);
                    length = 0;
                }
            }
        }
    }


}
