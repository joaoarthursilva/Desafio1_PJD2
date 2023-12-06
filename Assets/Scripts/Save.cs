using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

public class Save : MonoBehaviour
{
    public enum DerivedFromPrimitive
    {
        None,
        Cube,
        Sphere,
        Capsule
    }

    public enum ColliderType
    {
        None,
        Default,
        Trigger
    }

    [Serializable]
    public class GameObjectClass
    {
        public string name;
        public string prefabName;
        public TransformClass transformData;
        public DerivedFromPrimitive primitiveData;
        public string materialData;
        public ColliderType colliderData;
        public List<string> otherComponentsData;
    }

    [Serializable]
    public class TransformClass
    {
        public float[] position;
        public float[] scale;
        public float[] rotation;

        public static TransformClass FromTransform(Transform transform)
        {
            TransformClass transformClass = new()
            {
                position = transform.position.ToFloatArray(),
                scale = transform.localScale.ToFloatArray(),
                rotation = transform.rotation.eulerAngles.ToFloatArray()
            };

            return transformClass;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            SerializeScene();
        if (Input.GetKeyDown(KeyCode.Delete))
            DeleteScene();
        if (Input.GetKeyDown(KeyCode.L))
            DeserializeScene();
    }

    [SerializeField] private List<GameObjectClass> gameObjects = new();
    [SerializeField] private GameObject[] allGameObjects;
    public List<GameObjectClass> goList;

    private void SerializeScene()
    {
        allGameObjects = FindObjectsOfType<GameObject>();

        foreach (var go in allGameObjects)
        {
            goList.Add(new GameObjectClass
            {
                name = go.name,
                prefabName = GetPrefabName(go),
                transformData = TransformClass.FromTransform(go.transform),
                primitiveData = GetPrimitiveData(go),
                materialData = GetMaterialData(go),
                colliderData = GetColliderType(go)
            });
        }

        var jsonString = JsonConvert.SerializeObject(goList, Formatting.Indented);

        var jsonFilePath = Application.dataPath + $"/Scripts/scene.json";
        File.WriteAllText(jsonFilePath, jsonString);
    }

    private static string GetMaterialData(GameObject p_gameObject)
    {
        if (!p_gameObject.TryGetComponent(out Renderer rend)) return string.Empty;
        return rend.sharedMaterial != null ? rend.sharedMaterial.name : string.Empty;
    }

    private static DerivedFromPrimitive GetPrimitiveData(GameObject go)
    {
        if (go.TryGetComponent(out CubePrimitiveTag cubeTag)) return DerivedFromPrimitive.Cube;
        return go.TryGetComponent(out CapsulePrimitiveTag capsuleTag)
            ? DerivedFromPrimitive.Capsule
            : DerivedFromPrimitive.None;
    }

    private static ColliderType GetColliderType(GameObject go)
    {
        if (go.TryGetComponent(out Collider col))
            return col.isTrigger ? ColliderType.Trigger : ColliderType.Default;

        return ColliderType.None;
    }

    private static string GetPrefabName(UnityEngine.Object go)
    {
        return PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.Regular
            ? go.name
            : string.Empty;
    }

    private void DeleteScene()
    {
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            if (go != gameObject)
                Destroy(go);
        }
    }

    private void DeserializeScene()
    {
        var jsonString = File.ReadAllText(Application.dataPath + $"/Scripts/scene.json");

        gameObjects.Clear();
        gameObjects = JsonConvert.DeserializeObject<List<GameObjectClass>>(jsonString);
        var go = new GameObject();
        var instantiatedGos = new List<GameObject>();

        foreach (var gameObjectClass in gameObjects)
        {
            if (string.IsNullOrEmpty(gameObjectClass.prefabName))
            {
                switch (gameObjectClass.primitiveData)
                {
                    case DerivedFromPrimitive.Cube:
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.AddComponent<CubePrimitiveTag>();
                        break;
                    case DerivedFromPrimitive.Capsule:
                        go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        go.AddComponent<CapsulePrimitiveTag>();
                        break;
                    case DerivedFromPrimitive.Sphere:
                        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        break;
                    case DerivedFromPrimitive.None:
                        break;
                }

                if (!string.IsNullOrEmpty(gameObjectClass.materialData) ||
                    gameObjectClass.materialData == "Default-Material (Instance)")
                {
                    if (go.TryGetComponent(out Renderer rend))
                    {
                        rend.material = (Material) Resources.Load("Materials/" + gameObjectClass.materialData,
                            typeof(Material));
                    }
                }
            }
            else
            {
                if ((GameObject) Resources.Load("Prefabs/" + gameObjectClass.prefabName) is null) continue;
                go = Instantiate((GameObject) Resources.Load("Prefabs/" + gameObjectClass.prefabName));
            }

            if (go != null)
            {
                go.name = gameObjectClass.name;

                go.transform.position = gameObjectClass.transformData.position.ToVector3();
                go.transform.localScale = gameObjectClass.transformData.scale.ToVector3();
                go.transform.rotation =
                    Quaternion.Euler(gameObjectClass.transformData.rotation.ToVector3());


                if (gameObjectClass.colliderData != ColliderType.None)
                {
                    if (go.TryGetComponent(out Collider collid))
                    {
                        collid.isTrigger = gameObjectClass.colliderData == ColliderType.Default ? false : true;
                    }
                }
            }

            instantiatedGos.Add(go);
        }

        FindObjectOfType<CameraController>().target = FindObjectOfType<CameraTargetTag>().transform;
    }
}