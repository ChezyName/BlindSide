using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad
{
    public static bool Save(string saveName,object saveData)
    {
        BinaryFormatter f = GetBinaryFormatter();

        if(!Directory.Exists(Application.persistentDataPath + "/config/" + saveName + ".save"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/config/" + saveName + ".save");
        }

        string path = Application.persistentDataPath + "/config/" + saveName + ".save";

        FileStream file = File.Create(path);
        //Write To FileStream
        file.Close();
        return true;
    }

    public static bool SaveJson(string saveName,string saveData)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/config"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/config");
        }

        string path = Application.persistentDataPath + "/config/" + saveName;

        if (!File.Exists(path))
        {
            File.Create(path);
        }

        File.WriteAllText(path, saveData);
        return true;
    }

    public static string LoadJson(string saveName)
    {
        string path = Application.persistentDataPath + "/config/" + saveName;
        //Debug.Log(File.Exists(path));

        if (!File.Exists(path))
        {
            return null;
        }

        string data = File.ReadAllText(path);
        Debug.Log(data);
        return data;
    }

    public static object Load(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        BinaryFormatter f = GetBinaryFormatter();

        FileStream file = File.Open(path, FileMode.Open);

        try
        {
            object save = f.Deserialize(file);
            file.Close();
            return save;
        }
        catch{
            Debug.LogErrorFormat("Failed to load File at {0}", path);
            file.Close();
            return null;
        }
    }

    private static BinaryFormatter GetBinaryFormatter()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        return formatter;
    }
}