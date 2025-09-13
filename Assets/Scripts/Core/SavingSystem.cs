
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SavingSystem
{
    public static void Save(string id, int value)
    {
        PlayerPrefs.SetInt(id, value);
        PlayerPrefs.Save();
    }

    public static void Save(List<string> listId, ISavableData structure)
    {
		Type structType = structure.GetType();
        FieldInfo[] publicFields = structType.GetFields();
		for(int i = 0; i < listId.Count; i++)
		{
			Save(listId[i], (int)publicFields[i].GetValue(structure));
		}
    }
    
    public static int Load(string id)
    {
	    return PlayerPrefs.GetInt(id);
    }
    
    public static List<int> Load(List<string> listId)
    {
	    List<int> res = new();
	    for(int i = 0; i < listId.Count; i++)
	    {
		    res.Add(Load(listId[i]));
	    }

	    return res;
    }
}

