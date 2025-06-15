using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class CellularAutomata2 : MonoBehaviour
{
    [Header("Tilemaps and Tiles")]
    [SerializeField] Tilemap foregroundTilemap;
    [SerializeField] Tilemap backgroundTilemap;
    [SerializeField] RuleTile ruleTile;
    [SerializeField] TileBase groundTile;
    [SerializeField] TileBase backgroundTile;

    [Header("Map Settings")]
    [SerializeField] int width = 50;
    [SerializeField] int height = 50;
    [SerializeField] float InitFillPercent = 0.45f;
    [SerializeField] int softlyCount = 5;

    [Header("Ground Line Settings")]
    [SerializeField] int MinArea = 4;
    [SerializeField] int MaxWidthLimit = 8;

    private bool[,] map;
    private int[,] regions;

    void Start()
    {
        CreateMap();
    }

    public void CreateMap()
    {
        foregroundTilemap.ClearAllTiles(); 
        backgroundTilemap.ClearAllTiles();
        GenerateMap();
        ConnectRegions();
        PlaceBackground();
        PlaceTiles();
    }

    void GenerateMap()
    {
        map = new bool[width, height];
        RandomFillMap();

        for (int i = 0; i < softlyCount; i++)
            SmoothMap();
    }

    void RandomFillMap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = Random.value < InitFillPercent;
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = GetSurroundWallCount(x, y) > 4;
    }

    int GetSurroundWallCount(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    if ((dx != 0 || dy != 0) && map[nx, ny]) count++;
                }
                else count++; // out-of-bounds = wall
            }
        return count;
    }

    void ConnectRegions()
    {
        regions = new int[width, height];
        int regionId = 1;
        List<List<Vector2Int>> allRegions = new();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (!map[x, y] && regions[x, y] == 0)
                    allRegions.Add(FloodFill(x, y, regionId++));

        if (allRegions.Count <= 1) return;

        var mainRegion = allRegions.OrderByDescending(r => r.Count).First();

        foreach (var region in allRegions)
        {
            if (region == mainRegion) continue;
            var from = FindClosest(region, mainRegion);
            var to = FindClosest(mainRegion, region);
            CreatePassage(from, to);
        }
    }

    List<Vector2Int> FloodFill(int x, int y, int id)
    {
        List<Vector2Int> region = new();
        Queue<Vector2Int> q = new();
        q.Enqueue(new Vector2Int(x, y));
        regions[x, y] = id;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            region.Add(cur);

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                int nx = cur.x + dir.x, ny = cur.y + dir.y;
                if (nx >= 0 && ny >= 0 && nx < width && ny < height && !map[nx, ny] && regions[nx, ny] == 0)
                {
                    regions[nx, ny] = id;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
        return region;
    }

    Vector2Int FindClosest(List<Vector2Int> a, List<Vector2Int> b)
    {
        Vector2Int result = a[0];
        float minDist = float.MaxValue;

        foreach (var p1 in a)
            foreach (var p2 in b)
            {
                float d = (p1 - p2).sqrMagnitude;
                if (d < minDist) { minDist = d; result = p1; }
            }

        return result;
    }

    void CreatePassage(Vector2Int from, Vector2Int to)
    {
        Vector2Int cur = from;
        while (cur != to)
        {
            map[cur.x, cur.y] = false;

            if (cur.x != to.x) cur.x += cur.x < to.x ? 1 : -1;
            else if (cur.y != to.y) cur.y += cur.y < to.y ? 1 : -1;
        }
    }

    void PlaceBackground()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                backgroundTilemap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
    }

    void PlaceTiles()
    {
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y])
                    foregroundTilemap.SetTile(new Vector3Int(x, y, 0), ruleTile);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (visited[x, y] || map[x, y]) continue;

                int maxWidth = 0;
                while (x + maxWidth < width && !map[x + maxWidth, y] && !visited[x + maxWidth, y])
                    maxWidth++;

                int maxHeight = 0;
                while (y + maxHeight < height)
                {
                    bool rowOk = true;
                    for (int i = 0; i < maxWidth; i++)
                        if (map[x + i, y + maxHeight] || visited[x + i, y + maxHeight])
                        {
                            rowOk = false;
                            break;
                        }

                    if (!rowOk) break;
                    maxHeight++;
                }

                if (maxWidth >= MinArea && maxHeight >= MinArea)
                {
                    int clampedWidth = Mathf.Min(maxWidth, MaxWidthLimit);
                    int targetRow = y + MinArea / 2;
                    for (int dx = 0; dx < clampedWidth; dx++)
                    {
                        var pos = new Vector3Int(x + dx, targetRow, 0);
                        foregroundTilemap.SetTile(pos, groundTile);
                        visited[x + dx, targetRow] = true;
                    }
                }
            }
    }
}
