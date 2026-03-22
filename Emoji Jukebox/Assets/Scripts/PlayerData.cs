using System;

[Serializable]
public class PlayerData
{
    public string playerName;
    public int score;

    public PlayerData(string name)
    {
        playerName = name;
        score = 0;
    }
}