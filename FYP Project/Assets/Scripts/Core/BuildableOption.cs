using UnityEngine;

[System.Serializable]
public class BuildableOption
{
    public string name;
    public GameObject prefab;
    public BuildType type;
    public int cost;
}

public enum BuildType
{
    Wall,
    Turret,
    Trap
}