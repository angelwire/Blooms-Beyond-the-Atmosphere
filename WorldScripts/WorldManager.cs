//created: 9-30-19
//updated: 11-2-19
//version: .03
//author Vance Howald, William Jones

/*TODO
 * FlowerProperties make singleton 
 * maxDistance is traingular instead of square
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;
    public GameObject[,] grid;
    public List<GameObject> loadedChunks;
    public int viewRadius; //excluding where player is standing, so a 3x3 would be 1, 5x5 would be 2, etc
    public int[] playerLoc;
    public Vector3 worldCenter = Vector3.zero;
    public int chunkWidth = 10;
    public float maxDistance;
    public int[] gridSize = new int[2];
    public Placeable[,] placableGrid;
    public int FlowerWidthPerChunk;
    public float flowerPopDensitiy;
    public float worldTimer = 0.0f;
    public Vector3 worldOrgin;
    public float baseSpawnTime = 1.0f;
    public float randomSpawnRange = 10.0f;
    public LayerMask unplaceableLayer;
    public LayerMask placeableLayer; //flower colliders
    public LayerMask world;
    public List<GameObject> flowersToRepos;
    PriorityQueue<FlowerObj, float> flowerSpawnList = new PriorityQueue<FlowerObj, float>();
    private int flowerGridXLen;
    private int flowerGridYLen;
    public GameObject cover;
    Unplacable unplacableHolder; //things tagged hoverable and hoverblock will be added to it
    public float atmosphereHeight;

    public bool doMove = false;

    //The amount of curvature to apply to the world and the objects
    private float worldCurvature = .018f;
    [SerializeField] float maxWorldCurvature = .1f;
    [SerializeField] float minWorldCurvature = .02f;

    //How far away from the center of a cell a flower can be (0 being none and 1 being totally on the edge)
    public static float flowerMaxCenterDistance = .4f;

    //The flower properties script thatis connected to the world
    FlowerProperties fp;

    public bool homeBaseMenuIsShowing = false;

    public bool useRandomWorldGen = false;

    public static int currentPlanetID = 0;

    private Vector3 zeroZeroPos;
    private bool isOnAlienPlanet;
    public int oldX=2;
    public int oldY=2;
    private Vector3 oldPlayerPos;

    // Start is called before the first frame update
    void Start()
    {
        if (WorldManager.instance != null)
        {
            Destroy(this);
        }
        WorldManager.instance = this;//singleton

        isOnAlienPlanet = WarpDriveController.instance.currentPlanetID == Galaxy.HOME_PLANET_ID;

        //The flower properties that can be accessed later on
        fp = GameObject.Find("World").GetComponent<FlowerProperties>();
        zeroZeroPos = transform.GetChild(0).position;
        unplacableHolder = new GameObject().AddComponent<Unplacable>();//.GetComponent<Unplacable>();
        //unplacableHolder = new GameObject().AddComponent<Unplacable>().GetComponent<Unplacable>();
        //oldX = gridSize[0] / 2;
        //oldY = gridSize[1] / 2;
        maxDistance = viewRadius * (Mathf.Sqrt(2 * (chunkWidth * chunkWidth)));//this should probably be fixed
        //maxDistance = viewRadius * (Mathf.Sqrt(2 * (chunkWidth * chunkWidth)));//this should probably be fixed
        if (viewRadius > 1)
        {
            maxDistance -= 2f;//rounds off view distance
        }
        //maxDistance = 10000000;
        grid = new GameObject[gridSize[0], gridSize[1]];
        for (int y = 0; y < gridSize[1]; y++)//loop thru all grid elements. They must be sorted by Y and then by X, each increasing in value. This can be made looser but I didn't
        {
            for (int x = 0; x < gridSize[0]; x++)
            {
                grid[x, y] = transform.GetChild((y * gridSize[0]) + x).gameObject;//link it
                loadedChunks.Add(transform.GetChild((y * gridSize[0]) + x).gameObject);//since it's set to active add it to loaded chunks this will be unloaded later
                grid[x, y].gameObject.GetComponent<Chunk>().worldLoc = new int[2] { x, y };//give chunk its pos
            }
        }
        oldPlayerPos = grid[oldX, oldY].transform.position;
        flowerGridXLen = FlowerWidthPerChunk * gridSize[0];
        flowerGridYLen = FlowerWidthPerChunk * gridSize[1];
        placableGrid = new Placeable[flowerGridXLen, flowerGridYLen];//initialize flower grid
        float chunkOverFlower = (float)chunkWidth / FlowerWidthPerChunk;
        float chunkOverFlowerHalf = chunkOverFlower / 2;
        worldOrgin = grid[0, 0].transform.position - new Vector3(chunkWidth / 2, 0, chunkWidth / 2) + new Vector3(chunkOverFlowerHalf, 0, chunkOverFlowerHalf); //this is needed to find where the ground selector needs to go
        if (useRandomWorldGen)
        {
            gameObject.GetComponent<TileTest>().genGround(FlowerWidthPerChunk, gridSize[0], gridSize[1], flowerGridXLen, flowerGridYLen);
            GameObject newCover = new GameObject("cover");
            float xLength = gridSize[0] * chunkWidth;
            float yLength = gridSize[1] * chunkWidth;
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    GameObject currentBorderChunk = new GameObject("BorderChunk");
                    currentBorderChunk.transform.parent = newCover.transform;
                    float newX = xLength * xOffset;
                    float newY = yLength * yOffset;
                    for (int yIndex = 0; yIndex < gridSize[1]; yIndex++)
                    {
                        for (int xIndex = 0; xIndex < gridSize[0]; xIndex++)
                        {
                            GameObject newGridPiece = Instantiate(grid[xIndex, yIndex].gameObject, grid[xIndex, yIndex].transform.position-new Vector3(0,100,0), grid[xIndex, yIndex].transform.rotation, currentBorderChunk.transform);
                            newGridPiece.transform.position += new Vector3(newX, 100, newY);
                            Destroy(newGridPiece.GetComponent<MeshCollider>());
                            Destroy(newGridPiece.GetComponent<Chunk>());
                        }
                    }
                }
            }
            /*newCover.transform.position = worldOrgin;
            if (gridSize[0] % 2 == 0)
            {
                newCover.transform.position += new Vector3(chunkWidth, 0, 0);
            }
            if (gridSize[1] % 2 == 0)
            {
                newCover.transform.position += new Vector3(0, 0, chunkWidth);
            }*/
            cover = newCover;
        }
        int flowerPop = 0;
        float rayHeight = 20;
        if (WarpDriveController.instance != null)
        {
            if (WarpDriveController.instance.targetPlanetID == Galaxy.HOME_PLANET_ID)//there's a bug with the archways and since items can't be placed on planets this'll do
            {
                rayHeight = 10.5f;//avoid archway   
            }
        }
        for (int y = 0; y < flowerGridYLen; y++)//flower grid also follows quadrant 1 rules, sorted by Y and then by X, each increasing in value
        {
            for (int x = 0; x < flowerGridXLen; x++)
            {
                RaycastHit hit;
                Ray ray = new Ray(new Vector3(worldOrgin.x + (x * chunkOverFlower), worldOrgin.y - 10, worldOrgin.z + (y * chunkOverFlower)), Vector3.up);
                if (Physics.Raycast(ray, out hit, rayHeight, unplaceableLayer, QueryTriggerInteraction.Collide) || placableGrid[x, y] is Unplacable)//check for unplacable
                {
                    placableGrid[x, y] = unplacableHolder;
                }
                else if (Physics.Raycast(ray, out hit, rayHeight, placeableLayer, QueryTriggerInteraction.Collide))//check for flower
                {
                    hit.transform.gameObject.GetComponent<Placeable>().init(x, y);
                    placableGrid[x, y] = hit.transform.gameObject.GetComponent<Placeable>();
                }
            }
        }
        if (PlayerPrefs.HasKey("ONLINE_WORLD"))
        {
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("onlinePlanet"))
            {
                PlayerPrefs.DeleteKey("ONLINE_WORLD");
            }
            else
            {
                string toLoad = PlayerPrefs.GetString("ONLINE_WORLD");
                PlayerPrefs.DeleteKey("ONLINE_WORLD");
                download(toLoad);
            }
        }
        else if (PlayerPrefs.HasKey(WarpDriveController.instance.currentPlanetID + "_save"))
        {
            loadInventory();
            loadWorld(WarpDriveController.instance.currentPlanetID);
        }
        else
        {
            loadInventory();
            if (WarpDriveController.instance.currentPlanetID == Galaxy.HOME_PLANET_ID)//there's a bug with the archways and since items can't be placed on planets this'll do
            {
                rayHeight = 11.5f;//avoid archway   
            }
            for (int y = 0; y < flowerGridYLen; y++)//flower grid also follows quadrant 1 rules, sorted by Y and then by X, each increasing in value
            {
                for (int x = 0; x < flowerGridXLen; x++)
                {
                    if(placableGrid[x, y]==null)
                    {
                        //find the flower's location in the chunck
                        Transform parent = grid[x / FlowerWidthPerChunk, y / FlowerWidthPerChunk].transform;
                        if (Random.value <= flowerPopDensitiy)//should flower spawn
                        {
                            GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, parent);
                            FlowerObj currentFlowerObj = flower.GetComponent<FlowerObj>();
                            //Disabled print to speed up start time
                            //print(currentFlowerObj.gameObject);

                            //Get the elements count so it doesn't have to be hard coded
                            Planet.PlanetType pt = WarpDriveController.instance.GetDestinationPlanet().getType();

                            currentFlowerObj.init(fp.getRandomFlowerIDForPlanet(pt).ToString("X8"), new int[] { x, y }, parent);

                            currentFlowerObj.alive = true;
                            placableGrid[x, y] = currentFlowerObj;
                            flowerGen(currentFlowerObj);
                            flowerPop++;
                        }
                    }
                }
            }
            float remainingFlowers = (flowerGridXLen * flowerGridYLen * flowerPopDensitiy) - flowerPop;
            while (remainingFlowers > 0.9f)
            {
                int trialCount = 0;
                for (; trialCount < 20; trialCount++)
                {
                    int randomX = Random.Range(0, flowerGridXLen);
                    int randomY = Random.Range(0, flowerGridYLen);
                    if (placableGrid[randomX, randomY] == null)
                    {
                        Transform parrent = transform.GetChild((randomX / chunkWidth) + ((randomY / chunkWidth) * gridSize[1]));
                        GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, parrent);
                        FlowerObj currentFlowerObj = flower.GetComponent<FlowerObj>();
                        //Disabled print to speed up start time
                        //print(currentFlowerObj.gameObject);

                        Planet.PlanetType pt = WarpDriveController.instance.GetDestinationPlanet().getType();

                        currentFlowerObj.init(fp.getRandomFlowerIDForPlanet(pt).ToString("X8"), new int[] { randomX, randomY }, parrent);//get random flower
                        currentFlowerObj.alive = true;
                        placableGrid[randomX, randomY] = currentFlowerObj;
                        flowerGen(currentFlowerObj);
                        remainingFlowers--;
                        break;
                    }
                }
                if (trialCount >= 20)
                {
                    break;
                }
            }
            for (int i = 0; i <= 2; i++)//for loop to do world processing to give planets history
            {
                for (int y = 0; y < flowerGridYLen; y++)//flower grid also follows quadrant 1 rules, sorted by Y and then by X, each increasing in value
                {
                    for (int x = 0; x < flowerGridXLen; x++)
                    {
                        if (placableGrid[x, y] == null || placableGrid[x, y].isPlaceable())
                        {
                            updateFlowerPos(x, y);
                        }
                    }
                }
                while (flowerSpawnList.head != null)//if there's a flower in the to spawn list and the world timer is above 
                {
                    GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, flowerSpawnList.head.data.parent);
                    FlowerObj currentFlowerObj = flower.GetComponent<FlowerObj>();
                    currentFlowerObj.init(flowerSpawnList.head.data);
                    currentFlowerObj.alive = true;
                    placableGrid[flowerSpawnList.head.data.flowerGridPos[0], flowerSpawnList.head.data.flowerGridPos[1]] = currentFlowerObj;
                    flowerGen(currentFlowerObj);//add it in
                    RecalcAfterAdd(flowerSpawnList.head.data.flowerGridPos[0], flowerSpawnList.head.data.flowerGridPos[1]);//update the flower grid
                    currentFlowerObj.timeToSpawn = 0;
                    //flowerSpawnList.RemoveFront();
                }/*
            flowerSpawnList = new PriorityQueue<FlowerObj, float>();
            for (int y = 0; y < flowerGridYLen; y++)//flower grid also follows quadrant 1 rules, sorted by Y and then by X, each increasing in value
            {
                for (int x = 0; x < flowerGridXLen; x++)
                {
                    if (placableGrid[x, y] is FlowerObj && !placableGrid[x, y].isLivingFlower())
                    {
                        try
                        {
                            FlowerObj currentFlowerObj = (FlowerObj)placableGrid[x, y];
                            GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, currentFlowerObj.parent);
                            FlowerObj newFlowerObj = flower.GetComponent<FlowerObj>();
                            newFlowerObj.init(currentFlowerObj);
                            placableGrid[x, y] = newFlowerObj;
                            flowerGen(newFlowerObj);
                            newFlowerObj.timeToSpawn = 0;
                        }
                        catch
                        {
                            print("error");
                        }
                    }
                }
            }*/
            }

            PostDecorationController[] postList = GameObject.FindObjectsOfType<PostDecorationController>();
            foreach (PostDecorationController p in postList)
            {
                p.updateConnectors();
            }
        }

        

        //Set the initial curvature
        setWorldCurvature(worldCurvature);
    }

    // Update is called once per frame
    void Update()
    {

        worldTimer += Time.deltaTime;
        //Update shader time global
        //Shader.SetGlobalFloat("_Time", worldTimer);

        while (flowerSpawnList.head != null && worldTimer >= flowerSpawnList.head.data.timeToSpawn)//if there's a flower in the to spawn list and the world timer is above 
        {
            GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, flowerSpawnList.head.data.parent);
            FlowerObj currentFlowerObj = flower.GetComponent<FlowerObj>();
            currentFlowerObj.init(flowerSpawnList.head.data);
            currentFlowerObj.alive = true;
            placableGrid[flowerSpawnList.head.data.flowerGridPos[0], flowerSpawnList.head.data.flowerGridPos[1]] = currentFlowerObj;
            flowerGen(currentFlowerObj); //add it in
            RecalcAfterAdd(flowerSpawnList.head.data.flowerGridPos[0], flowerSpawnList.head.data.flowerGridPos[1]);//update the flower grid
            currentFlowerObj.timeToSpawn = 0;
            //flowerSpawnList.RemoveFront();
        }
        

    }

    /// <summary>
    /// Every chunk is in sub chunks which are then moved arround individually
    /// </summary>
    /// <param name="newX"></param>
    /// <param name="newY"></param>
    /// <param name="toMove"></param>
    private void moveChunks(int newX, int newY, Transform toMove)
    {
        Vector3 newPlayerPos = grid[newX, newY].transform.position;
        if (newY != oldY)
        {
            int yTodo = newPlayerPos.z > oldPlayerPos.z ? newY - oldY : oldY - newY;
            if (yTodo < 0)
            {
                yTodo += gridSize[1];
            }
            int negitiveHelper = newPlayerPos.z > oldPlayerPos.z ? 1 : -1;
            int start = newY - ((gridSize[1] / 2) * negitiveHelper);
            if (gridSize[1] % 2 == 0&& negitiveHelper==1)
            {
                start++;
            }
            int target = start + (yTodo * negitiveHelper);
            for (; yTodo > 0; yTodo--)
            {
                start -= negitiveHelper;
                for (int currentX = 0; currentX < gridSize[0]; currentX++)
                {
                    toMove.GetChild(mod(currentX, gridSize[0]) + (mod(start, gridSize[1]) * gridSize[1])).position += new Vector3(0, 0, chunkWidth * gridSize[1]) * negitiveHelper;
                }
            }

        }
        if (newX != oldX)
        {
            int xTodo = newPlayerPos.x > oldPlayerPos.x ? newX - oldX : oldX - newX;
            if (xTodo < 0)
            {
                xTodo += gridSize[0];
            }
            int negitiveHelper = newPlayerPos.x > oldPlayerPos.x ? 1 : -1;
            int start = newX - ((gridSize[0] / 2) * negitiveHelper);
            if (gridSize[0] % 2 == 0 && negitiveHelper == 1)
            {
                start++;
            }
            int target = start + (xTodo * negitiveHelper);
            for (; xTodo > 0; xTodo--)
            {
                start -= negitiveHelper;
                for (int currentY = 0; currentY < gridSize[1]; currentY++)
                {
                    toMove.GetChild(mod(start, gridSize[0]) + (mod(currentY, gridSize[1]) * gridSize[1])).position += new Vector3(chunkWidth * gridSize[0], 0, 0) * negitiveHelper;
                }
            }

        }
    }

    public void updateLoadedChunks(int newX, int newY)//when the player steps into the next grid this updates what chunks should be 
    {
        isOnAlienPlanet = WarpDriveController.instance.currentPlanetID != Galaxy.HOME_PLANET_ID && WarpDriveController.instance.currentPlanetID != Galaxy.ONLINE_PLANET_ID;
        moveChunks(newX, newY, transform);
        //unload(newX, newY);//the corners may need to be removed so do this again
        if (!isOnAlienPlanet)
        {
            cover.transform.position = grid[newX, newY].transform.position;
            /*if (gridSize[0] % 2 == 0)
                cover.transform.position -= new Vector3(chunkWidth, 0f, 0);
            if (gridSize[1] % 2 == 0)
                cover.transform.position -= new Vector3(0f, 0f, chunkWidth);*/
        }
        else
        {
            for (int i = 0; i < cover.transform.childCount; i++)
            {
                moveChunks(newX, newY, cover.transform.GetChild(i));
            }
        }
        foreach (GameObject flower in flowersToRepos)
        {
            RaycastHit hit;
            Ray ray = new Ray(flower.transform.position + new Vector3(0, 100, 0), Vector3.down);
            if (Physics.Raycast(ray, out hit, 200.0f, world, QueryTriggerInteraction.Ignore))
            {
                flower.GetComponent<setCurvaturePosition>().baseYPosition = hit.point.y;
            }
        }
        oldPlayerPos = grid[newX, newY].transform.position;
        oldX = newX;
        oldY = newY;

    }

    public int mod(int a, int n)//adds in the moddulus opperator, a % n
    {
        int result = a % n;
        if ((result < 0 && n > 0) || (result > 0 && n < 0))
        {
            result += n;
        }
        return result;
    }

    private GameObject flowerGen(FlowerObj flowerObj)//makes new flower //William and vance
    {

        float chunkOverFlower = (float)chunkWidth / FlowerWidthPerChunk;
        int[] chunkRealtivePos = new int[] { mod(flowerObj.flowerGridPos[0], FlowerWidthPerChunk), mod(flowerObj.flowerGridPos[1], FlowerWidthPerChunk) };
        GameObject flower = flowerObj.gameObject;
        //GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, grid[flowerObj.flowerGridPos[0]/FlowerWidthPerChunk, flowerObj.flowerGridPos[1]/FlowerWidthPerChunk].transform);

        //This line added by william for UI inventory testing
        //flower.GetComponent<Flower>().flowerGridInstance = flowerObj;


        flower.transform.Translate((-.5f * chunkWidth) + ((float)chunkWidth / (2.0f * FlowerWidthPerChunk)) + ((float)chunkOverFlower * chunkRealtivePos[0]), 0, (-.5f * chunkWidth) + ((float)chunkWidth / (2.0f * FlowerWidthPerChunk)) + ((float)chunkOverFlower * chunkRealtivePos[1])); //moves it to the flower spot
        Vector3 jiggle = new Vector3((Random.value * chunkOverFlower) - (chunkOverFlower / 2), 0, (Random.value * chunkOverFlower) - (chunkOverFlower / 2));//moves it somewhere within flower spot
        jiggle *= flowerMaxCenterDistance;//scales it back to the center so it doesn't spawn right on the edge
        flower.transform.Translate(jiggle);//jiggle it within the chunk for more random distribution
        flower.transform.Rotate(0, Random.value * 360.0f, 0);//random rot
        GameObject stemObject = flower.transform.GetChild(0).gameObject;
        RaycastHit hit;
        Ray ray = new Ray(flower.transform.position+new Vector3(0,100,0), Vector3.down);
        if (Physics.Raycast(ray, out hit, 200.0f, world, QueryTriggerInteraction.Ignore))
        {
            flower.GetComponent<setCurvaturePosition>().baseYPosition = hit.point.y;
        }
        else
        {
            flowersToRepos.Add(flower);
        }
        //Destroy the flower before building a new one
        //Destroy the stem that destroys everything
        if (stemObject != null)
        {
            GameObject.DestroyImmediate(stemObject);
        }
        try
        {
            //Instantiate the given stem object
            stemObject = Instantiate(fp.flowerStem[flowerObj.stemIndex[0]], flower.transform);
        }
        catch
        {
            //Instantiate the given stem object
            stemObject = Instantiate(fp.flowerStem[flowerObj.stemIndex[0]], flower.transform);
        }
        Material stemMat = fp.stemMaterial;
        stemObject.GetComponent<MeshRenderer>().material = stemMat;
        stemObject.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.stemIndex[1]];
        GameObject petalObject = null;
        //Add leaves and petal to the stem
        bool petalHasBeenFound = false;
        foreach (Transform child in stemObject.transform)
        {
            if (!petalHasBeenFound && child.gameObject.name.ToString().Equals("spetal"))
            {
                //It's the petal so it has been found
                petalHasBeenFound = true;
                //Now spawn it
                petalObject = Instantiate(fp.flowerPetal[flowerObj.petalIndex[0]], child);
                Material petalMat = fp.petalMaterial;
                petalObject.GetComponent<MeshRenderer>().material = petalMat;
                petalObject.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.petalIndex[1]];
            }
            else
            {
                //It's a leaf so spawn it
                GameObject leaf = Instantiate(fp.flowerLeaf[flowerObj.leafIndex[0]], child);
                Material leafMat = fp.leafMaterial;
                leaf.GetComponent<MeshRenderer>().material = leafMat;
                leaf.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.leafIndex[1]];
            }
        }
        //Add pistil to the petal
        GameObject pistilObject = Instantiate(fp.flowerPistil[flowerObj.pistilIndex[0]], petalObject.transform.GetChild(0));
        Material pistilMat = fp.pistilMaterial;
        pistilObject.GetComponent<MeshRenderer>().material = pistilMat;
        pistilObject.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.pistilIndex[1]];

        flower.name = flowerObj.flowerGridPos[0] + " " + flowerObj.flowerGridPos[1];
        combineMeshes(flower.transform);
        resizeBoxCollider(flower);
        return flower;
    }

    public void resizeBoxCollider(GameObject toResize) //resizes the box collider to the given game object
    {
        BoxCollider boxCol = toResize.GetComponent<BoxCollider>();

        //Add a box collider if there isn't one
        if (boxCol == null)
        {
            boxCol = toResize.AddComponent<BoxCollider>();//take the box colider
        }

        boxCol.isTrigger = true;
        Transform parrent = toResize.transform.parent;
        toResize.transform.SetParent(null);
        Vector3 pos = toResize.transform.position;//move back to the center because otherwise the math is all off
        toResize.transform.position = Vector3.zero;
        Vector3 rot = toResize.transform.eulerAngles;
        toResize.transform.rotation = Quaternion.identity;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);//bounds for figuring out the size
        Renderer thisRenderer = toResize.transform.GetComponent<Renderer>();
        bounds.Encapsulate(thisRenderer.bounds);
        boxCol.center = bounds.center - toResize.transform.position;
        boxCol.size = bounds.size;
        Transform[] allDescendants = toResize.transform.GetComponentsInChildren<Transform>();
        foreach (Transform desc in allDescendants)//get all the transforms inside and adjust the colider with them
        {
            Renderer childRenderer = desc.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
            boxCol.center = bounds.center - toResize.transform.position;
            boxCol.size = bounds.size;
        }
        toResize.transform.eulerAngles = rot;//move it back
        toResize.transform.position = pos;
        toResize.transform.SetParent(parrent);
    }

    /// <summary>
    /// Need to use the setter so that it will automatically update the shader value in the ground prefabs
    /// </summary>
    /// <param name="inCurvature">Should be between 0 and .01</param>
    public void setWorldCurvature(float inCurvature)//William
    {
        //Set the curvature value
        worldCurvature = inCurvature;

        //Set the global curvature value in the shaders
        Shader.SetGlobalFloat("_Curvature", inCurvature);
    }
    public float getWorldCurvature() { return worldCurvature; }//William

    public void clearFlowerAt(int x, int y)//remove flower from loc and clear neighbors
    {
        placableGrid[x, y] = null;//put some impossible values in remporarily, this is needed for neighbor checking 
        for (int deltaY = -1; deltaY <= 1; deltaY++)//now that this square no longer has a flower the squares arround it must also forget
        {
            for (int deltaX = -1; deltaX <= 1; deltaX++)
            {
                if (deltaX + deltaY == -1 || deltaX + deltaY == 1)
                {
                    int trueXi = trueX(deltaX + x);
                    int trueYi = trueY(deltaY + y);
                    if (placableGrid[trueXi, trueYi] == null || placableGrid[trueXi, trueYi].isPlaceable())
                    {
                        updateFlowerPos(trueXi, trueYi);
                    }
                }
            }
        }
        updateFlowerPos(x, y);
        //print("cleared "+x+" "+y);
    }

    public void RecalcAfterAdd(int x, int y)//if a flower is added it needs to update all flowers arround it so
    {
        removeFromFlowerList(x, y);//remove self from spawn list because there's now a flower in this spot
        for (int deltaY = -1; deltaY <= 1; deltaY++)//now that this square no longer has a flower the squares arround it must also forget
        {
            for (int deltaX = -1; deltaX <= 1; deltaX++)
            {
                if (deltaX + deltaY == -1 || deltaX + deltaY == 1)
                {
                    int trueXi = trueX(deltaX + x);
                    int trueYi = trueY(deltaY + y);
                    if (placableGrid[trueXi, trueYi] == null || placableGrid[trueXi, trueYi].isPlaceable())//if there's no flower there
                    {
                        updateFlowerPos(trueXi, trueYi);//update
                    }
                }
            }
        }
    }

    public void updateFlowerPos(int x, int y)//looks at neighbors and sees if 
    {
        removeFromFlowerList(x, y);
        List<string> flowerIds = new List<string>();
        int leftX = mod((x - 1), flowerGridXLen);
        int rightX = mod((x + 1), flowerGridXLen);
        if ((placableGrid[trueX(x - 1), y] != null && placableGrid[trueX(x + 1), y] != null && placableGrid[trueX(x - 1), y].isLivingFlower() && placableGrid[trueX(x + 1), y].isLivingFlower()) || (placableGrid[x, trueY(y - 1)] != null && placableGrid[x, trueY(y + 1)] != null && placableGrid[x, trueY(y - 1)].isLivingFlower() && placableGrid[x, trueY(y + 1)].isLivingFlower()))
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    if (yOffset + xOffset < -1 || yOffset + xOffset > 1 || yOffset + xOffset == 0)
                    {
                        continue;
                    }
                    if (placableGrid[trueX(x + xOffset), trueY(y + yOffset)] != null && placableGrid[trueX(x + xOffset), trueY(y + yOffset)].isLivingFlower())
                    {
                        flowerIds.Add(((FlowerObj)placableGrid[trueX(x + xOffset), trueY(y + yOffset)]).getFlowerIndex().ToString("X8"));
                    }
                }
            }
            string toPlant = "";
            for (int i = 0; i < flowerIds[0].Length; i += 2)
            {
                char lockedChar = ' ';
                for (int j = 0; j < flowerIds.Count - 1; j++)
                {
                    for (int k = j + 1; k < flowerIds.Count; k++)
                    {
                        if (flowerIds[j][i] == flowerIds[k][i])
                        {
                            if (lockedChar != ' ')
                            {
                                if (Random.value > .5f)
                                {
                                    lockedChar = flowerIds[k][i];
                                }
                            }
                            else
                            {
                                lockedChar = flowerIds[k][i];
                            }
                        }
                    }
                }
                if (lockedChar != ' ')
                {
                    toPlant += lockedChar;
                }
                else
                {
                    toPlant += flowerIds[Random.Range(0, flowerIds.Count)][i];
                }
                toPlant += flowerIds[Random.Range(0, flowerIds.Count)][i + 1];
            }
            GameObject temp = new GameObject();
            FlowerObj newFlower = temp.AddComponent<FlowerObj>();
            newFlower.init(toPlant, new int[] { }, null);
            newFlower.timeToSpawn = baseSpawnTime + Random.Range(0, randomSpawnRange) + worldTimer;
            newFlower.flowerGridPos = new int[] { x, y };
            newFlower.parent = grid[x / FlowerWidthPerChunk, y / FlowerWidthPerChunk].transform;

            placableGrid[x, y] = newFlower;

            //Disabled the print to speed up start time
            //print(newFlower is FlowerObj);
            flowerSpawnList.Add((FlowerObj)placableGrid[x, y], ((FlowerObj)placableGrid[x, y]).timeToSpawn);

            //Spawn a particle
            //Get the chunk that the particles should be parented to
            Transform chunkTransform = grid[x / FlowerWidthPerChunk, y / FlowerWidthPerChunk].transform;
            GameObject particleObject = Instantiate(fp.growingFlowerEmitter, chunkTransform);
            float chunkOverFlower = (float)chunkWidth / FlowerWidthPerChunk;
            int[] chunkRealtivePos = new int[] { mod(x, FlowerWidthPerChunk), mod(y, FlowerWidthPerChunk) };
            //GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, grid[flowerObj.flowerGridPos[0]/FlowerWidthPerChunk, flowerObj.flowerGridPos[1]/FlowerWidthPerChunk].transform);

            //This line added by william for UI inventory testing
            //flower.GetComponent<Flower>().flowerGridInstance = flowerObj;


            particleObject.transform.Translate((-.5f * chunkWidth) + ((float)chunkWidth / (2.0f * FlowerWidthPerChunk)) + ((float)chunkOverFlower * chunkRealtivePos[0]), 0, (-.5f * chunkWidth) + ((float)chunkWidth / (2.0f * FlowerWidthPerChunk)) + ((float)chunkOverFlower * chunkRealtivePos[1])); //moves it to the flower spot
            particleObject.transform.eulerAngles = new Vector3(-90, 0, 0);
            newFlower.particles = particleObject;

            RaycastHit hit;
            Ray ray = new Ray(particleObject.transform.position + new Vector3(0, 100, 0), Vector3.down);
            if (Physics.Raycast(ray, out hit, 200.0f, world, QueryTriggerInteraction.Ignore))
            {
                particleObject.GetComponent<setCurvaturePosition>().baseYPosition = hit.point.y;
            }

        }
        else
        {
            placableGrid[x, y] = null;
        }
    }

    public void removeFromFlowerList(int x, int y)
    {
        if (placableGrid[x, y] != null && placableGrid[x, y].isFlower)
        {
            GameObject temp = flowerSpawnList.FindAndRemove((FlowerObj)placableGrid[x, y], ((FlowerObj)placableGrid[x, y]).timeToSpawn + 1.0f);
            if (temp != null)
            {
                GameObject.Destroy(temp.GetComponent<FlowerObj>().particles);
                DestroyImmediate(temp);
            }
        }
    }

    public int[] worldPointToFlowerGrid(Vector3 pos) //Converts the given world point to the x,y, grid point
    {
        float chunkOverFlower = (float)chunkWidth / FlowerWidthPerChunk;
        float chunkOverFlowerHalf = chunkOverFlower / 2;
        pos -= worldOrgin;// + new Vector3(chunkOverFlowerHalf, 0, chunkOverFlowerHalf);
        if (pos.x < 0.0)
        {
            pos.x -= chunkOverFlowerHalf;
        }
        else
        {
            pos.x += chunkOverFlowerHalf;
        }
        if (pos.z < 0.0)
        {
            pos.z -= chunkOverFlowerHalf;
        }
        else
        {
            pos.z += chunkOverFlowerHalf;
        }
        int[] intToReturn = new int[] { mod((int)(pos.x / chunkOverFlower), flowerGridXLen), mod((int)(pos.z / chunkOverFlower), flowerGridYLen) };
        return intToReturn;
    }

    public Vector3 worldPointToGridOrgin(Vector3 pos) //Converts the world point to the grid origin
    {
        Vector3 toReturn = Vector3.zero;
        float chunkOverFlower = (float)chunkWidth / FlowerWidthPerChunk;
        float halfChunk = chunkOverFlower / 2;
        float toAdd = pos.x > worldOrgin.x ? chunkOverFlower : -chunkOverFlower;
        for (float distance = pos.x - worldOrgin.x; -halfChunk > distance || distance > halfChunk; distance = pos.x - (worldOrgin.x + (toReturn.x)))
        {
            toReturn.x += toAdd;
        }
        toAdd = pos.z > worldOrgin.z ? chunkOverFlower : -chunkOverFlower;
        for (float distance = pos.z - worldOrgin.z; -halfChunk > distance || distance > halfChunk; distance = pos.z - (worldOrgin.z + (toReturn.z)))
        {
            toReturn.z += toAdd;
        }
        toReturn += worldOrgin;
        return toReturn;
    }

    public int trueX(int x)//get the actual grid pos
    {
        return mod(x, flowerGridXLen);
    }
    public int trueY(int y)//gets the actual grid pos
    {
        return mod(y, flowerGridYLen);
    }


    public void sortWorld()//reorders the hierarchy to make the world loader work 
    {
        bool isSorted = false;
        do//sort by x
        {
            isSorted = true;
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                if (transform.GetChild(i).transform.position.x > transform.GetChild(i + 1).transform.position.x)
                {
                    transform.GetChild(i + 1).SetSiblingIndex(i);
                    isSorted = false;
                }
            }
        } while (!isSorted);
        isSorted = false;
        do//then by z (called y here)
        {
            isSorted = true;
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                if (transform.GetChild(i).transform.position.z > transform.GetChild(i + 1).transform.position.z)
                {
                    transform.GetChild(i + 1).SetSiblingIndex(i);
                    isSorted = false;
                }
            }
        } while (!isSorted);
        float xPos = transform.GetChild(0).transform.position.x;
        float zPos = transform.GetChild(0).transform.position.z;
        int xCount = 0;
        int zCount = 0;
        transform.GetChild(0).gameObject.name = "0x0";
        for (int i = 1; i < transform.childCount; i++)//rename everything
        {
            if (Mathf.Approximately(transform.GetChild(i).transform.position.z, zPos))
            {
                xCount++;
            }
            if (Mathf.Approximately(transform.GetChild(i).transform.position.x, xPos))
            {
                zCount++;
                xCount = 0;
                zPos = transform.GetChild(i).transform.position.z;
            }
            transform.GetChild(i).gameObject.name = xCount + "x" + zCount;
        }
    }

    public void combineMeshes(Transform toCombine) //Combines the meshes to increase performance
    {
        combineAllMeshes(toCombine);
        return;
    }



    //Test to see if combining all meshes and materials into a single game object will make the game run faster or not
    //William Jones: 1-1-20
    private void combineAllMeshes(Transform toCombine) //Combines the meshes to increase performance
    {
        toCombine = toCombine.GetChild(0);

        //Store the flower's transform
        Vector3 oldPosition = toCombine.position;
        Quaternion oldRotation = toCombine.rotation;

        //Reset the flower's transform
        toCombine.position = Vector3.zero;
        toCombine.rotation = Quaternion.identity;

        //Array to hold the mesh filters for all the children
        List<MeshFilter> meshFilters = new List<MeshFilter>(toCombine.GetComponentsInChildren<MeshFilter>());

        //If there are meshes to combine
        if (meshFilters.Count > 0)
        {
            //Set up the combine instance array
            CombineInstance[] combineInstance = new CombineInstance[meshFilters.Count];

            //The colors for the material
            Color cStem = Color.white;
            Color cLeaf = Color.white;
            Color cPetal = Color.white;
            Color cPistil = Color.white;

            //Loop through the mesh filters
            for (int ii = 0; ii < meshFilters.Count; ii += 1)
            {
                //Set the current combine instance mesh and transform
                combineInstance[ii].mesh = meshFilters[ii].mesh;
                combineInstance[ii].transform = meshFilters[ii].transform.localToWorldMatrix;

                //Adjust the UVs and set the material color
                float scale = .5f;
                float xOffset = 0f;
                float yOffset = 0f;
                if (meshFilters[ii].gameObject.name.Contains("leaf"))
                {
                    xOffset = .5f;
                    yOffset = .5f;
                    cLeaf = meshFilters[ii].GetComponent<MeshRenderer>().material.color;
                }
                else if (meshFilters[ii].gameObject.name.Contains("stem"))
                {
                    yOffset = .5f;
                    cStem = meshFilters[ii].GetComponent<MeshRenderer>().material.color;
                }
                else if (meshFilters[ii].gameObject.name.Contains("pistil"))
                {
                    xOffset = .5f;
                    cPistil = meshFilters[ii].GetComponent<MeshRenderer>().material.color;
                }
                else
                {
                    cPetal = meshFilters[ii].GetComponent<MeshRenderer>().material.color;
                }
                Vector2[] uvs = meshFilters[ii].mesh.uv;
                for (int jj = 0; jj < uvs.Length; jj += 1)
                {
                    uvs[jj].x = (uvs[jj].x * scale) + xOffset;
                    uvs[jj].y = (uvs[jj].y * scale) + yOffset;
                }

                meshFilters[ii].mesh.uv = uvs;
            }
            //Combine the mesh and add it
            toCombine.GetComponent<MeshFilter>().mesh = new Mesh();
            toCombine.GetComponent<MeshFilter>().mesh.CombineMeshes(combineInstance, true);

            //Find the material to apply to the mesh that will be created
            Material useMaterial = fp.flowerMaterial;
            Material mat;
            if (toCombine.GetComponent<Material>() == null)
            {
                mat = Instantiate(useMaterial);
            }
            else
            {
                mat = toCombine.GetComponent<Material>();
            }

            mat.SetColor("_LeafColor", cLeaf);
            mat.SetColor("_StemColor", cStem);
            mat.SetColor("_PetalColor", cPetal);
            mat.SetColor("_PistilColor", cPistil);

            toCombine.GetComponent<MeshRenderer>().material = mat;

            //Loop through the mesh filters
            for (int ii = 0; ii < meshFilters.Count; ii += 1)
            {
                //Make sure the root game object isn't being destroyed
                if (meshFilters[ii].gameObject != toCombine.gameObject)
                {
                    //Destroy each mesh filter object
                    Destroy(meshFilters[ii].gameObject);
                }
            }
        }

        //Now reset the position
        toCombine.position = oldPosition;
        toCombine.rotation = oldRotation;
    }
    //End mesh combine test




    void OnApplicationQuit() //Resets the curvature and daylight when the game quits (used for the editor)
    {
        //Clear the curvature when the game ends
        setWorldCurvature(0);

        //Clear the post processing and night time effects
        GameObject.Find("Sky").GetComponent<SkyController>().setDaylightWithAtmosphere(1, 1);
    }

    public AsyncOperation saveWorld(int worldToSave)
    {
        AsyncOperation operation = new AsyncOperation();
        if (worldToSave != Galaxy.ONLINE_PLANET_ID)
        {
            GameObject.FindObjectOfType<WorldLocScript>().isSaving = true;
            Vector3 oldPlayerPos = GameObject.FindObjectOfType<WorldLocScript>().gameObject.transform.position;
            Vector3[,] oldPoses = new Vector3[gridSize[0], gridSize[1]];
            for (int y = 0; y < gridSize[1]; y++)
            {
                for (int x = 0; x < gridSize[0]; x++)
                {
                    oldPoses[x,y] = transform.GetChild((y * gridSize[1]) + x).position;
                    transform.GetChild((y * gridSize[1]) + x).position = zeroZeroPos + (new Vector3(x, 0, y) * chunkWidth);
                }
            }
            PlayerPrefs.SetString(worldToSave + "_save", "yee");
            for (int x = 0; x < flowerGridXLen; x++)
            {
                for (int y = 0; y < flowerGridYLen; y++)
                {
                    PlayerPrefs.DeleteKey(worldToSave + "x" + x + "y" + y + "_save");
                    if (placableGrid[x, y] != null && placableGrid[x, y].isPlaceable())
                    {
                        continue;
                    }
                    if (placableGrid[x, y] != null && !(placableGrid[x, y] is Unplacable))//make sure it isn't unplaceables
                    {
                        PlayerPrefs.SetString(worldToSave + "x" + x + "y" + y + "_save", placableGrid[x, y].save());
                    }
                }
            }

            for (int y = 0; y < gridSize[1]; y++)
            {
                for (int x = 0; x < gridSize[0]; x++)
                {
                    transform.GetChild((y * gridSize[1]) + x).position = oldPoses[x, y];
                }
            }

            GameObject.FindObjectOfType<WorldLocScript>().gameObject.transform.position=oldPlayerPos;
            GameObject.FindObjectOfType<WorldLocScript>().isSaving = false;


            UIControllerScript uIControllerS = GameObject.Find("UICanvas").GetComponent<UIControllerScript>();
            for (int i = 0; i < 10; i++)
            {
                PlayerPrefs.DeleteKey("inventory_" + i);
                if (uIControllerS.getInventoryDataAtSlot(i) != null)
                {
                    if (uIControllerS.getInventoryDataAtSlot(i).isFlower)
                    {
                        PlayerPrefs.SetString("inventory_" + i, uIControllerS.getInventoryDataAtSlot(i).save() + "|" + uIControllerS.getInventoryCountAtSlot(i));
                    }
                    else
                    {
                        PlayerPrefs.SetString("inventory_" + i, "1|" + uIControllerS.getInventoryDataAtSlot(i).id + "|" + uIControllerS.getInventoryCountAtSlot(i));
                    }
                }
            }

            HomeInventoryData.instance.save();

            Galaxy.instance.saveGalaxy();

            PlayerPrefs.SetInt("jetpack_level", WarpDriveController.instance.getJetpackLevel());

            PlayerPrefs.SetInt("quest_progress", GameObject.Find("UICanvas").GetComponent<UIControllerScript>().currentQuestId);
            PlayerPrefs.SetInt("money", GameObject.Find("FPSController").GetComponent<FlowerPlayerControler>().money);
            if (worldToSave == Galaxy.HOME_PLANET_ID)
            {
                HomeBaseController.instance.saveEconomy();
            }
        }
        return operation;
    }

    public void loadWorld(int toLoad)
    {
        for (int x = 0; x < flowerGridXLen; x++)
        {
            for (int y = 0; y < flowerGridYLen; y++)
            {
                if (placableGrid[x, y] != null)
                {
                    Destroy(placableGrid[x, y].gameObject);
                    placableGrid[x, y] = null;
                }
                Transform parent = grid[x / FlowerWidthPerChunk, y / FlowerWidthPerChunk].transform;
                if (PlayerPrefs.HasKey(toLoad + "x" + x + "y" + y + "_save"))
                {
                    string[] processed = PlayerPrefs.GetString(toLoad + "x" + x + "y" + y + "_save").Split('|');
                    if (processed[0] == "0")
                    {
                        GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, parent);
                        FlowerObj newFlowerObj = flower.GetComponent<FlowerObj>();
                        newFlowerObj.init(new int[] { int.Parse(processed[1]), int.Parse(processed[2]) }, new int[] { int.Parse(processed[3]), int.Parse(processed[4]) }, new int[] { int.Parse(processed[5]), int.Parse(processed[6]) }, new int[] { int.Parse(processed[7]), int.Parse(processed[8]) }, new int[] { int.Parse(processed[9]), int.Parse(processed[10]) }, parent);
                        newFlowerObj.alive = true;
                        newFlowerObj.flowerGridPos = new int[] { x, y };
                        placableGrid[x, y] = newFlowerObj;
                        flowerGen(newFlowerObj);
                        flower.transform.position = new Vector3(float.Parse(processed[11]), float.Parse(processed[12]), float.Parse(processed[13]));
                        flower.transform.eulerAngles = new Vector3(float.Parse(processed[14]), float.Parse(processed[15]), float.Parse(processed[16]));
                    }
                    else
                    {
                        GameObject newObj = Instantiate(fp.getDecorationObject(uint.Parse(processed[1])), parent);
                        Placeable p = newObj.GetComponent<Placeable>();
                        p.flowerGridPos = new int[] { x, y };
                        if (p.additionalLoad(PlayerPrefs.GetString(toLoad + "x" + x + "y" + y + "_save")))
                        {
                            newObj.transform.position = new Vector3(float.Parse(processed[2]), float.Parse(processed[3]), float.Parse(processed[4]));
                            newObj.transform.eulerAngles = new Vector3(float.Parse(processed[5]), float.Parse(processed[6]), float.Parse(processed[7]));
                        }
                        else
                        {
                            Destroy(newObj);
                        }
                        placableGrid[x, y] = p;
                    }
                }
                else
                {
                    //placableGrid[x, y] = new FlowerObj(new int[] { -1, -1 }, new int[] { -1, -1 }, new int[] { -1, -1 }, new int[] { -1, -1 }, new int[] { x, y }, parent);//put some impossible values in remporarily, this is needed for neighbor checking later
                }
            }
        }
        flowerSpawnList = new PriorityQueue<FlowerObj, float>();
        /*for (int y = 0; y < flowerGridYLen; y++)//flower grid also follows quadrant 1 rules, sorted by Y and then by X, each increasing in value
        {
            for (int x = 0; x < flowerGridXLen; x++)
            {
                if (placableGrid[x, y] != null && placableGrid[x,y].isLivingFlower())
                {
                    RecalcAfterAdd(x, y);
                }
            }
        }*/
        ConnectablePlaceable[] connectList = ConnectablePlaceable.FindObjectsOfType<ConnectablePlaceable>();
        for (int i = 0; i < connectList.Length; i++)
        {
            connectList[i].load();
        }
        for (int i = 0; i < connectList.Length; i++)
        {
            connectList[i].updateConnectors(false);
        }
        /*
        List<PostDecorationController> forceConnectors = new List<PostDecorationController>();
        PostDecorationController[] postList = PostDecorationController.FindObjectsOfType<PostDecorationController>();
        for (int i = 0; i < postList.Length; i++)
        {
            postList[i].updateConnectors();
            postList[i].load();
            for (int j = 0; j < 4; j++)
            {
                if (postList[i].forcedConnectors[j])
                {
                    forceConnectors.Add(postList[i]);
                    break;
                }
            }
        }
        int[,] offsetTable = { { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 0 } };
        for (int i = 0; i < forceConnectors.Count; i++)
        {
            forceConnectors[i].load();
            for (int j = 0; j < 4; j++)
            {
                if (postList[i].forcedConnectors[j])
                {
                    Placeable toUse = placableGrid[trueX(postList[i].flowerGridPos[0] + offsetTable[j, 0]), trueY(postList[i].flowerGridPos[1] + offsetTable[j, 1])];
                    postList[i].hardConnectToPlaceable(toUse, j * 90);
                }
            }
        }*/
        loadInventory();
        for (int y = 0; y < flowerGridYLen; y++)//flower grid also follows quadrant 1 rules, sorted by Y and then by X, each increasing in value
        {
            for (int x = 0; x < flowerGridXLen; x++)
            {
                if (placableGrid[x, y] == null || placableGrid[x, y].isPlaceable())
                {
                    updateFlowerPos(x, y);
                }
            }
        }
    }

    public void loadInventory()
    {
        UIControllerScript uIControllerS = GameObject.Find("UICanvas").GetComponent<UIControllerScript>();
        for (int i = 0; i < 10; i++)
        {
            if (PlayerPrefs.HasKey("inventory_" + i))
            {
                string[] processed = PlayerPrefs.GetString("inventory_" + i).Split('|');
                if (processed[0] == "0")
                {
                    GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject);
                    FlowerObj newFlowerObj = flower.GetComponent<FlowerObj>();
                    newFlowerObj.init(new int[] { int.Parse(processed[1]), int.Parse(processed[2]) }, new int[] { int.Parse(processed[3]), int.Parse(processed[4]) }, new int[] { int.Parse(processed[5]), int.Parse(processed[6]) }, new int[] { int.Parse(processed[7]), int.Parse(processed[8]) }, new int[] { -1, -1 }, null);
                    newFlowerObj.alive = true;
                    uIControllerS.setSlot(i, newFlowerObj);
                    Destroy(flower);
                }
                else
                {
                    GameObject newObj = Instantiate(fp.getDecorationObject(uint.Parse(processed[1])));
                    Placeable p = newObj.GetComponent<Placeable>();
                    uIControllerS.setSlot(i, p);
                    Destroy(newObj);
                }
                uIControllerS.setSlotCount(i, int.Parse(processed[processed.Length - 1]));
            }
        }
        if (PlayerPrefs.HasKey("money"))
        {
            GameObject.Find("FPSController").GetComponent<FlowerPlayerControler>().money = PlayerPrefs.GetInt("money");
        }
    }

    public int[] getFlowerGridSize()
    {
        return new int[] { flowerGridXLen, flowerGridYLen };
    }

    /// <summary>
    /// Activates the home base menu by telling the objects to pause
    /// and loading the home base scene on top of the current scene
    /// </summary>
    public void ActivateHomeBaseMenu()
    {
        if (homeBaseMenuIsShowing == false)
        {
            homeBaseMenuIsShowing = true;
            GameObject player = GameObject.Find("FPSController");
            GameObject uiController = GameObject.Find("UICanvas");
            if (player != null && uiController != null)
            {
                //Make sure all the appropriate objects are told to pause the game
                //player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().sit(gameObject);
                player.GetComponent<FlowerPlayerControler>().isViewingHomeBase = true;

                //Pause the game without popping up the pause menu
                uiController.GetComponent<UIControllerScript>().pauseWithoutMenu();
            }
            //Load the inventory screen
            SceneManager.LoadSceneAsync("HomeScreenScene", LoadSceneMode.Additive);
        }
    }

    public void DeactivateHomeBaseMenu()
    {
        if (homeBaseMenuIsShowing == true)
        {
            homeBaseMenuIsShowing = false;
            //Do other stuff to let the home world scene know that the inventory is closed
            GameObject player = GameObject.Find("FPSController");
            GameObject uiController = GameObject.Find("UICanvas");
            if (player != null && uiController != null)
            {
                //Give the player commands
                player.GetComponent<FlowerPlayerControler>().isViewingHomeBase = false;
                player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().stand();

                //Give the world manager commands
                uiController.GetComponent<UIControllerScript>().resumeGame();
            }
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("HomeScreenScene"));
        }
    }

    public void download(string toUse)
    {
        string[] bigList = toUse.Split('[');
        for (int x = 0; x < flowerGridXLen; x++)
        {
            for (int y = 0; y < flowerGridYLen; y++)
            {
                int index = (x * flowerGridYLen) + y;
                if (placableGrid[x, y] != null)
                {
                    Destroy(placableGrid[x, y].gameObject);
                    placableGrid[x, y] = null;
                }
                if (bigList[index] != "")
                {
                    Transform parent = grid[x / FlowerWidthPerChunk, y / FlowerWidthPerChunk].transform;

                    string[] processed = bigList[index].Split('|');
                    if (processed[0] == "0")
                    {
                        GameObject flower = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject, parent);
                        FlowerObj newFlowerObj = flower.GetComponent<FlowerObj>();
                        newFlowerObj.init(new int[] { int.Parse(processed[1]), int.Parse(processed[2]) }, new int[] { int.Parse(processed[3]), int.Parse(processed[4]) }, new int[] { int.Parse(processed[5]), int.Parse(processed[6]) }, new int[] { int.Parse(processed[7]), int.Parse(processed[8]) }, new int[] { int.Parse(processed[9]), int.Parse(processed[10]) }, parent);
                        newFlowerObj.alive = true;
                        newFlowerObj.flowerGridPos = new int[] { x, y };
                        placableGrid[x, y] = newFlowerObj;
                        flowerGen(newFlowerObj);
                        flower.transform.position = new Vector3(float.Parse(processed[11]), float.Parse(processed[12]), float.Parse(processed[13]));
                        flower.transform.eulerAngles = new Vector3(float.Parse(processed[14]), float.Parse(processed[15]), float.Parse(processed[16]));
                    }
                    else
                    {
                        GameObject newObj = Instantiate(fp.getDecorationObject(uint.Parse(processed[1])), parent);
                        Placeable p = newObj.GetComponent<Placeable>();
                        p.flowerGridPos = new int[] { x, y };
                        if (p.additionalLoad(bigList[index]))
                        {
                            newObj.transform.position = new Vector3(float.Parse(processed[2]), float.Parse(processed[3]), float.Parse(processed[4]));
                            newObj.transform.eulerAngles = new Vector3(float.Parse(processed[5]), float.Parse(processed[6]), float.Parse(processed[7]));
                        }
                        else
                        {
                            Destroy(newObj);
                        }
                        placableGrid[x, y] = p;
                    }

                }
            }
        }

        flowerSpawnList = new PriorityQueue<FlowerObj, float>();
        /*for (int y = 0; y < flowerGridYLen; y++)//flower grid also follows quadrant 1 rules, sorted by Y and then by X, each increasing in value
        {
            for (int x = 0; x < flowerGridXLen; x++)
            {
                if (placableGrid[x, y] != null && placableGrid[x,y].isLivingFlower())
                {
                    RecalcAfterAdd(x, y);
                }
            }
        }*/
        ConnectablePlaceable[] connectList = ConnectablePlaceable.FindObjectsOfType<ConnectablePlaceable>();
        for (int i = 0; i < connectList.Length; i++)
        {
            connectList[i].load();
        }
        for (int i = 0; i < connectList.Length; i++)
        {
            connectList[i].updateConnectors(false);
        }
    }

    public void makeUnplacable(int x, int y)
    {
        placableGrid[x, y] = unplacableHolder;
    }

    public float getPlanetWidth()
    {
        return FlowerWidthPerChunk * gridSize[0];
        //return 60f;
    }

    public float getPlanetHeight()
    {
        return FlowerWidthPerChunk * gridSize[1];
        //return 60f;
    }


    //Called from FirstPersonController every frame
    public void updateWorldCurvatureFromHeight(float height)
    {
        float lerpValue = height / instance.atmosphereHeight;
        float newCurvature = Mathf.Lerp(minWorldCurvature, maxWorldCurvature, lerpValue);
        WorldManager.instance.setWorldCurvature(newCurvature);
    }
}

