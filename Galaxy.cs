using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour
{
    public static Galaxy instance;
    public static int HOME_PLANET_ID;
    public static int ONLINE_PLANET_ID = 999;
    public List<Planet> planetList;
    public static int maxPlanets = 16;
    public Planet onlinePlanet;
    public bool coldPlanetVisited = false;
    public bool neonPlanetVisited = false;
    public bool hotPlanetVisited = false;


    // Awake is called before everything
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Galaxy.instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Galaxy.instance = this;//singleton

        onlinePlanet = new Planet(Planet.PlanetType.ONLINE, Galaxy.ONLINE_PLANET_ID, new Vector3(0, -20, 0));

        planetList = new List<Planet>();

        Planet homePlanet = new Planet(Planet.PlanetType.HOME, 0, Vector3.zero);
        homePlanet.setVisited(true);
        homePlanet.unlockPlanet();
        planetList.Add(homePlanet);

        for(int i=0; i<5;i++)
        {
            int pid = (i * 3);
            if (PlayerPrefs.HasKey("planet_" + (pid+1)))
            {
                planetList.Add(new Planet(PlayerPrefs.GetString("planet_"+(pid + 1)), pid + 1));
            }
            else
            {
                planetList.Add(new Planet(Planet.PlanetType.HOT, pid + 1, getRandomPosition(pid + 1, maxPlanets)));
            }

            if (PlayerPrefs.HasKey("planet_" + (pid + 2)))
            {
                planetList.Add(new Planet(PlayerPrefs.GetString("planet_" + (pid + 2)), pid+2));
            }
            else
            {
                planetList.Add(new Planet(Planet.PlanetType.COLD, pid + 2, getRandomPosition(pid + 2, maxPlanets)));
            }

            if (PlayerPrefs.HasKey("planet_" + (pid+3)))
            {
                planetList.Add(new Planet(PlayerPrefs.GetString("planet_" + (pid + 3)), pid+3));
            }
            else
            {
                planetList.Add(new Planet(Planet.PlanetType.NEON, pid + 3, getRandomPosition(pid + 3, maxPlanets)));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach(Planet p in planetList)
            {
                print("Galaxy planet " + p.getDescriptionText());
            }
        }
    }

    private Vector3 getRandomPosition(int planetID, int maximum)
    {
        float minPlanetDistance = 7;
        float maxPlanetDistance = 13;
        float planetDistance = Random.Range(minPlanetDistance, maxPlanetDistance);
        float placeAngle = (360.0f / maximum) * planetID + Random.Range(-7f, 7f);
        float xPos = Mathf.Cos(Mathf.Deg2Rad * placeAngle) * planetDistance;
        float zPos = Mathf.Sin(Mathf.Deg2Rad * placeAngle) * planetDistance;
        return new Vector3(xPos, Random.Range(-planetDistance, planetDistance), zPos);
    }

    //saves the planet positions
    public void saveGalaxy()
    {
        //Save
        for(int i=0; i<maxPlanets; i++)
        {
            PlayerPrefs.SetString("planet_" + i, planetList[i].getSaveString());
        }
    }

    //Uses the jetpack level to unlock a planet
    public void unlockPlanetByJetpackLevel()
    {
        int jpl = WarpDriveController.instance.getJetpackLevel();
        if (jpl > 0 && jpl < planetList.Count)
        {
            planetList[jpl].unlockPlanet();
        }
    }

    public void resetSave()
    {

        planetList = new List<Planet>();

        Planet homePlanet = new Planet(Planet.PlanetType.HOME, 0, Vector3.zero);
        homePlanet.setVisited(true);
        homePlanet.unlockPlanet();
        planetList.Add(homePlanet);
        for (int i = 0; i < 5; i++)
        {
            int pid = (i * 3);
            planetList.Add(new Planet(Planet.PlanetType.HOT, pid + 1, getRandomPosition(pid + 1, maxPlanets)));
            planetList.Add(new Planet(Planet.PlanetType.COLD, pid + 2, getRandomPosition(pid + 2, maxPlanets)));
            planetList.Add(new Planet(Planet.PlanetType.NEON, pid + 3, getRandomPosition(pid + 3, maxPlanets)));
        }
    }

}
