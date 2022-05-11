using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : Singleton<RoadGenerator>
{

    public GameObject RoadPrefab;
    private List<GameObject> roads = new List<GameObject>();
    public float maxSpeed = 10f;
    public float speed = 0f;
    public int maxRoadCount = 5;
    

    void Start()
    {
        PoolManager.Instance.Preload(RoadPrefab, 15);
        
        ResetLevel();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (speed == 0) 
        {
            return;
        }

        foreach (GameObject road in roads)
        {
            road.transform.position -= new Vector3(0, 0, speed * Time.deltaTime);
        }

        if (roads[0].transform.position.z < -30)
        {
            PoolManager.Instance.DeSpawn(roads[0]);
            roads.RemoveAt(0);
            
            CreateNextRoad();
        }
    }
    
    public void StartLevel()
    {
        speed = maxSpeed;
        SwipeManager.Instance.enabled = true;
        
    }


    public void ResetLevel()
    {
        speed = 0;
        while (roads.Count > 0)
        {
            Destroy(roads[0]);
            roads.RemoveAt(0);
        }

        for (int i = 0; i < maxRoadCount; i++)
        {
            CreateNextRoad();
        }
        SwipeManager.Instance.enabled = false;
        MapGenerator.Instance.ResetMaps();
    }

    private void CreateNextRoad()
    {
        Vector3 pos = Vector3.zero;
        if (roads.Count > 0)
        {
            pos = roads[roads.Count - 1].transform.position + new Vector3(0, 0, 30);
        }
        GameObject go = PoolManager.Instance.Spawn(RoadPrefab, pos, Quaternion.identity);
        go.transform.SetParent(transform);
        roads.Add(go);
    }
}
