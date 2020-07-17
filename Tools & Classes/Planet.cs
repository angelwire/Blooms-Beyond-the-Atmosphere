//Class to hold planet information
//William Jones 3-19-20
using System.Text;
using UnityEngine;
public class Planet
{
    public enum PlanetType { HOME, HOT, COLD, NEON, ONLINE }
    PlanetType type;
    int id;
    private Color color;
    private bool isVisited;
    private bool isLocked;
    private int seed;
    private string name;
    Vector3 galacticPosition;

    public Planet(PlanetType type, int inID, Vector3 position)
    {
        galacticPosition = position;
        id = inID;
        this.type = type;
        isLocked = true;
        isVisited = false;
        seed = Random.Range(0, 10000000);
        this.name = "";

        switch (this.type)
        {
            case Planet.PlanetType.HOME: this.color = new Color(0, .8f, 0); break;
            case Planet.PlanetType.COLD: this.color = new Color(.4f, .7f, 1); break;
            case Planet.PlanetType.HOT: this.color = new Color(1, .6f, .3f); break;
            case Planet.PlanetType.NEON: this.color = new Color(.8f, .3f, .7f); break;
            default: this.color = new Color(0, .1f, .15f); break;
        }
    }

    public Planet(string loadString, int inID)
    {
        string[] argument = loadString.Split('|');
        galacticPosition.x = float.Parse(argument[0]);
        galacticPosition.y = float.Parse(argument[1]);
        galacticPosition.z = float.Parse(argument[2]);
        this.type = (PlanetType)int.Parse(argument[3]);
        id = inID;
        isVisited = (argument[4] == "1");
        isLocked = (argument[5] == "1");
        seed = int.Parse(argument[6]);
        name = argument[7];

        switch (this.type)
        {
            case Planet.PlanetType.HOME: this.color = new Color(0, .8f, 0); break;
            case Planet.PlanetType.COLD: this.color = new Color(.4f, .7f, 1); break;
            case Planet.PlanetType.HOT: this.color = new Color(1, .6f, .3f); break;
            case Planet.PlanetType.NEON: this.color = new Color(.8f, .3f, .7f); break;
            default: this.color = new Color(0, .1f, .15f); break;
        }
    }

    public Color getColor()
    {
        return this.color;
    }

    public int getID()
    {
        return id;
    }

    public PlanetType getType()
    {
        return type;
    }

    public int getSeed()
    {
        return seed;
    }
    public void setSeed(int s)
    {
        seed = s;
    }

    public void setName(string inName)
    {
        this.name = inName;
    }

    public string getName()
    {
        return this.name;
    }

    public string getDescriptionText()
    {
        if (name == "")
        {
            StringBuilder sb = new StringBuilder();
            switch (this.type)
            {
                case Planet.PlanetType.HOME: sb.Append("Home planet "); break;
                case Planet.PlanetType.COLD: sb.Append("Cold planet "); break;
                case Planet.PlanetType.HOT: sb.Append("Hot planet "); break;
                case Planet.PlanetType.NEON: sb.Append("Neon planet "); break;
                default: sb.Append("Unknown planet "); break;
            }
            return sb.ToString();
        }
        else
        {
            return this.name;
        }
    }

    public Vector3 getGalacticPosition()
    {
        return galacticPosition;
    }

    public void setVisited(bool visited)
    {
        isVisited = visited;
    }

    public void unlockPlanet()
    {
        isLocked = false;
    }
    public void lockPlanet()
    {
        isLocked = true;
    }

    public bool planetIsLocked()
    {
        return isLocked;
    }

    public bool planetIsVisited()
    {
        return isVisited;
    }

    /// <summary>
    /// Returns a string that is used for saving and loading
    /// x|y|z|type|visited
    /// </summary>
    /// <returns></returns>
    public string getSaveString()
    {
        StringBuilder result = new StringBuilder();
        result.Append(galacticPosition.x);
        result.Append("|");
        result.Append(galacticPosition.y);
        result.Append("|");
        result.Append(galacticPosition.z);
        result.Append("|");
        result.Append(type.ToString("d"));
        result.Append("|");
        result.Append((isVisited ? "1" : "0"));
        result.Append("|");
        result.Append((isLocked ? "1" : "0"));
        result.Append("|");
        result.Append(seed);
        result.Append("|");
        result.Append(name);
        return result.ToString();
    }
}
