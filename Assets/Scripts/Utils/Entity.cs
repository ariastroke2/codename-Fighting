using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Entity
{
    public static List<GameObject> GetAll()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        LayerMask entityLayer = LayerMask.NameToLayer("Entity");
        
        GameObject[] allEntities = currentScene.GetRootGameObjects();
        List<GameObject> Entities = new();

        foreach (GameObject obj in allEntities) { 
            if(entityLayer.value == obj.layer)
            {
                Entities.Add(obj);
            }
        }
        return Entities;
    }

    public static void DisableCollision(Collider col)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        LayerMask entityLayer = LayerMask.NameToLayer("Entity");

        GameObject[] allEntities = currentScene.GetRootGameObjects();

        foreach (GameObject obj in allEntities)
        {
            if (entityLayer.value == obj.layer)
            {
                Collider otherCol = obj.GetComponent<Collider>();
                if(otherCol != null)
                    Physics.IgnoreCollision(col, otherCol);
            }
        }
    }
}
