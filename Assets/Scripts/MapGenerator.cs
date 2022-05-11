using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : Singleton<MapGenerator>
{
    private int itemSpace = 15;
    private int itemCountInMap = 5;
    public float laneOffset = 2.5f;
    private int coinsCountInItem = 10;
    private float coinsHeight = 0.5f;
    private float mapSize;

    enum TrackPos
    {
        Left = -1,
        Center = 0,
        Right = 1
    };

    enum CoinsStyle
    {
        Line,
        Jump,
        Ramp,
        None
    };

    
    public GameObject CoinPrefab;
    public GameObject ObstacleCarPrefab;
    public GameObject ObstacleMinePrefab;
    public GameObject ObstacleSpikesPrefab;
    public GameObject RampPrefab;

    public List<GameObject> maps = new List<GameObject>();
    public List<GameObject> activeMaps = new List<GameObject>();

    struct MapItem
    {
        public void SetValues(GameObject obstacle, TrackPos trackPos, CoinsStyle coinsStyle)
        {
            this.obstacle = obstacle;
            this.trackPos = trackPos;
            this.coinsStyle = coinsStyle;
        }
        public GameObject obstacle;
        public TrackPos trackPos;
        public CoinsStyle coinsStyle;
    }
    private void Awake()
    {
        mapSize = itemCountInMap * itemSpace;
        maps.Add(MakeMap1());
        maps.Add(MakeMap2());
        maps.Add(MakeMap3());
        maps.Add(MakeMap4());
        foreach (GameObject map in maps)
        {
            map.SetActive(false);
        }
    }

    void Start()
    {

    }
    
    // Update is called once per frame
    void Update()
    {
        if (RoadGenerator.Instance.speed == 0)
        {
            return;
        }
        foreach (GameObject map in activeMaps)
        {
            map.transform.position -= new Vector3(0, 0, RoadGenerator.Instance.speed * Time.deltaTime);
        }

        if (activeMaps[0].transform.position.z < -mapSize)
        {
            RemoveFirstActiveMap();
            AddActiveMap();
        }
        
    }

    public void ResetMaps()
    {
        while (activeMaps.Count > 0)
        {
            RemoveFirstActiveMap();
        }
        AddActiveMap();
        AddActiveMap();
    }

    private void AddActiveMap()
    {
        int r = Random.Range(0, maps.Count);
        GameObject go = maps[r];
        go.SetActive(true);
        foreach (Transform child in go.transform)
        {
            child.gameObject.SetActive(true);
        }

        go.transform.position = activeMaps.Count > 0
            ? activeMaps[activeMaps.Count - 1].transform.position + Vector3.forward * mapSize
            : new Vector3(0, 0, 10);
        maps.RemoveAt(r);
        activeMaps.Add(go);
    }

    private void RemoveFirstActiveMap()
    {
        activeMaps[0].SetActive(false);
        maps.Add(activeMaps[0]);
        activeMaps.RemoveAt(0);
    }
    
    private void CreateCoins(CoinsStyle coinsStyle, Vector3 pos, GameObject parrentObject)
    {
        Vector3 coinPos = Vector3.zero;
        if (coinsStyle == CoinsStyle.Line)
        {
            for (int i = -coinsCountInItem/2; i < coinsCountInItem/2; i++)
            {
                coinPos.y = coinsHeight;
                coinPos.z = i * ((float)itemSpace / coinsCountInItem);
                GameObject go = Instantiate(CoinPrefab, coinPos + pos, Quaternion.identity);
                go.transform.SetParent(parrentObject.transform);
            }
        }
        if (coinsStyle == CoinsStyle.Jump)
        {
            for (int i = -coinsCountInItem/2; i < coinsCountInItem/2; i++)
            {
                coinPos.y = Mathf.Max(-1/2f * Mathf.Pow(i,2) + 3, coinsHeight);
                coinPos.z = i * ((float)itemSpace / coinsCountInItem);
                GameObject go = Instantiate(CoinPrefab, coinPos + pos, Quaternion.identity);
                go.transform.SetParent(parrentObject.transform);
            }
        }
        if (coinsStyle == CoinsStyle.Ramp)
        {
            for (int i = -coinsCountInItem/2; i < coinsCountInItem/2; i++)
            {
                coinPos.y = Mathf.Min(Mathf.Max(0.8f * (i+3), coinsHeight),2.5f);
                coinPos.z = i * ((float)itemSpace / coinsCountInItem);
                GameObject go = Instantiate(CoinPrefab, coinPos + pos, Quaternion.identity);
                go.transform.SetParent(parrentObject.transform);
            }
        }
    }
    private GameObject MakeMap1()
    {
        GameObject result = new GameObject("Map1");
        result.transform.SetParent(transform);
        MapItem item = new MapItem();
        for (int i = 0; i < itemCountInMap; i++)
        {
            item.SetValues(null, TrackPos.Center, CoinsStyle.Line);
            if (i == 2)
            {
                item.SetValues(RampPrefab, TrackPos.Left, CoinsStyle.Ramp);
            }

            if (i == 3)
            {
                item.SetValues(ObstacleMinePrefab, TrackPos.Right, CoinsStyle.Jump);
            }

            if (i == 4)
            {
                item.SetValues(ObstacleSpikesPrefab, TrackPos.Right, CoinsStyle.Jump);
            }

            Vector3 obstaclePos = new Vector3((int)item.trackPos * laneOffset, 0, i * itemSpace);
            CreateCoins(item.coinsStyle, obstaclePos, result);
            if (item.obstacle != null)
            {
                GameObject go = Instantiate(item.obstacle, obstaclePos, Quaternion.identity);
                go.transform.SetParent(result.transform);
            }
        }
        
        return result;
    }
    private GameObject MakeMap2()
    {
        GameObject result = new GameObject("Map2");
        result.transform.SetParent(transform);
        MapItem item = new MapItem();
        for (int i = 0; i < itemCountInMap; i++)
        {
            item.SetValues(null, TrackPos.Center, CoinsStyle.Line);
            if (i == 0)
            {
                item.SetValues(ObstacleMinePrefab, TrackPos.Left, CoinsStyle.Jump);
            }
            if (i == 2)
            {
                item.SetValues(ObstacleCarPrefab, TrackPos.Right, CoinsStyle.None);
            }

            if (i == 3)
            {
                item.SetValues(ObstacleSpikesPrefab, TrackPos.Center, CoinsStyle.Jump);
            }

            if (i == 4)
            {
                item.SetValues(RampPrefab, TrackPos.Left, CoinsStyle.Ramp);
            }

            Vector3 obstaclePos = new Vector3((int)item.trackPos * laneOffset, 0, i * itemSpace);
            CreateCoins(item.coinsStyle, obstaclePos, result);
            if (item.obstacle != null)
            {
                GameObject go = Instantiate(item.obstacle, obstaclePos, Quaternion.identity);
                go.transform.SetParent(result.transform);
            }
        }
        
        return result;
    }
    private GameObject MakeMap3()
    {
        GameObject result = new GameObject("Map3");
        result.transform.SetParent(transform);
        MapItem item = new MapItem();
        for (int i = 0; i < itemCountInMap; i++)
        {
            item.SetValues(null, TrackPos.Center, CoinsStyle.Line);
            if (i == 0)
            {
                item.SetValues(ObstacleSpikesPrefab, TrackPos.Left, CoinsStyle.Jump);
            }
            if (i == 1)
            {
                item.SetValues(RampPrefab, TrackPos.Center, CoinsStyle.Ramp);
            }
            if (i == 2)
            {
                item.SetValues(ObstacleSpikesPrefab, TrackPos.Center, CoinsStyle.None);
            }

            if (i == 3)
            {
                item.SetValues(ObstacleMinePrefab, TrackPos.Right, CoinsStyle.Jump);
            }

            if (i == 4)
            {
                item.SetValues(RampPrefab, TrackPos.Left, CoinsStyle.Ramp);
            }

            Vector3 obstaclePos = new Vector3((int)item.trackPos * laneOffset, 0, i * itemSpace);
            CreateCoins(item.coinsStyle, obstaclePos, result);
            if (item.obstacle != null)
            {
                GameObject go = Instantiate(item.obstacle, obstaclePos, Quaternion.identity);
                go.transform.SetParent(result.transform);
            }
        }
        
        return result;
    }
    private GameObject MakeMap4()
    {
        GameObject result = new GameObject("Map4");
        result.transform.SetParent(transform);
        MapItem item = new MapItem();
        for (int i = 0; i < itemCountInMap; i++)
        {
            item.SetValues(null, TrackPos.Center, CoinsStyle.Line);
            if (i == 0)
            {
                item.SetValues(ObstacleMinePrefab, TrackPos.Left, CoinsStyle.Jump);
            }
            if (i == 1)
            {
                item.SetValues(ObstacleCarPrefab, TrackPos.Center, CoinsStyle.None);
            }
            if (i == 2)
            {
                item.SetValues(RampPrefab, TrackPos.Right, CoinsStyle.Ramp);
            }
            
            if (i == 3)
            {
                item.SetValues(ObstacleSpikesPrefab, TrackPos.Right, CoinsStyle.None);
            }

            Vector3 obstaclePos = new Vector3((int)item.trackPos * laneOffset, 0, i * itemSpace);
            CreateCoins(item.coinsStyle, obstaclePos, result);
            if (item.obstacle != null)
            {
                GameObject go = Instantiate(item.obstacle, obstaclePos, Quaternion.identity);
                go.transform.SetParent(result.transform);
            }
        }
        
        return result;
    }

    

    
}
