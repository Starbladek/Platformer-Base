using UnityEngine;
using System.Collections;

public class GenericObject : MonoBehaviour {

    float timerLength;

    public static GameObject CreateObject(Vector3 spawnPos, Sprite objectSprite, float timerLength)
    {
        GameObject newObject = new GameObject();
        newObject.AddComponent<SpriteRenderer>();
        newObject.AddComponent<GenericObject>();

        newObject.transform.position = spawnPos;
        newObject.GetComponent<SpriteRenderer>().sprite = objectSprite;
        newObject.GetComponent<GenericObject>().timerLength = timerLength;

        LeanTween.move(newObject, new Vector3(spawnPos.x, spawnPos.y + 0.15f, spawnPos.z), timerLength).setEase(LeanTweenType.easeOutCubic).setOnComplete(newObject.GetComponent<GenericObject>().DestroySelf);
        LeanTween.alpha(newObject, 0, timerLength).setEase(LeanTweenType.easeInCubic);

        return newObject;
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }

}
