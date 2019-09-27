using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Asha))]
[CanEditMultipleObjects]
public class AshaScriptEditor : Editor
{

    SerializedProperty exclamationPointSpriteProp;
    SerializedProperty widthProp;
    SerializedProperty heightProp;
    SerializedProperty centerProp;
    SerializedProperty showPatrolZoneProp;
    SerializedProperty leftEyeProp;
    SerializedProperty rightEyeProp;

    //When the object is selected
    void OnEnable()
    {
        //Debug.Log("Scene GUI : Enabled");

        //serializedObject properties are just variables from a given class (in this case, we're grabbing variables from the Asha script)
        exclamationPointSpriteProp = serializedObject.FindProperty("exclamationPointSprite");
        widthProp = serializedObject.FindProperty("patrolZoneWidth");
        heightProp = serializedObject.FindProperty("patrolZoneHeight");
        centerProp = serializedObject.FindProperty("patrolZoneCenter");
        showPatrolZoneProp = serializedObject.FindProperty("showPatrolZone");
        leftEyeProp = serializedObject.FindProperty("leftEyeBounds");
        rightEyeProp = serializedObject.FindProperty("rightEyeBounds");
    }

    //When the object is deselected
    /*void OnDisable()
    {
        Debug.Log("Scene GUI : Disabled");
    }*/

    //This is the method that is called everytime the inspector is drawn in Unity
    public override void OnInspectorGUI()
    {
        Asha ashaObject = (Asha)target;

        serializedObject.Update();  //Set serializedObject stream to what is in the actual class
        EditorGUILayout.PropertyField(exclamationPointSpriteProp);
        EditorGUILayout.PropertyField(widthProp);
        EditorGUILayout.PropertyField(heightProp);
        EditorGUILayout.PropertyField(centerProp);
        EditorGUILayout.PropertyField(showPatrolZoneProp);

        if (GUILayout.Button("Center Patrol Zone on Asha"))
        {
            Undo.RecordObject(ashaObject, "centered patrol zone");
            ashaObject.CenterPatrolZone();
        }

        EditorGUILayout.PropertyField(leftEyeProp);
        EditorGUILayout.PropertyField(rightEyeProp);
        serializedObject.ApplyModifiedProperties(); //Apply changes made to the serializedObject stream to the actual class

        SceneView.RepaintAll(); //Repaint the scene view with the changes made in the editor

        //Defining properties with custom names
        //widthProp.floatValue = EditorGUILayout.FloatField(new GUIContent("Width"), widthProp.floatValue);
        /*Asha ashaScript = (Asha)target;
        ashaScript.patrolZoneWidth = EditorGUILayout.FloatField("PZ Width", ashaScript.patrolZoneWidth);
        ashaScript.patrolZoneHeight = EditorGUILayout.FloatField("PZ Height", ashaScript.patrolZoneHeight);
        ashaScript.patrolZoneCenter = EditorGUILayout.Vector2Field("PZ Center", ashaScript.patrolZoneCenter);*/
    }

    //This uses Handles, which are drawn when the object is selected
    /*void OnSceneGUI()
    {
        Asha t = target as Asha;

        float width = widthProp.floatValue;
        float height = heightProp.floatValue;
        Vector2 center = centerProp.vector2Value;

        Vector3[] verts = new Vector3[]
        {
            new Vector3(center.x - width/2, center.y + height/2, 0),    //Top left corner
            new Vector3(center.x + width/2, center.y + height/2, 0),    //Top right corner
            new Vector3(center.x + width/2, center.y - height/2, 0),    //Bottom right corner
            new Vector3(center.x - width/2, center.y - height/2, 0)     //Bottom left corner
        };

        Handles.color = new Color(0, 0, 1, .25f);
        Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 1, 1, 0.2f), new Color(0, 0, 0, 1));
    }*/



}