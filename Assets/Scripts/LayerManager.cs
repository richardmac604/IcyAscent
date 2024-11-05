using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    public enum LayerType
    {
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,
        Climbable = 6,
        Unclimbable = 8,
        Slippery = 9,
        
    }

    private static readonly Dictionary<LayerType, string> LayerTypeNames = new Dictionary<LayerType, string>
    {
        { LayerType.Default, "Default" },
        { LayerType.TransparentFX, "TransparentFX" },
        { LayerType.IgnoreRaycast, "Ignore Raycast" },
        { LayerType.Water, "Water" },
        { LayerType.UI, "UI" },
        { LayerType.Unclimbable, "Unclimbable" },
        { LayerType.Slippery, "Slippery" },
        { LayerType.Climbable, "Climbable" }

    };

    public LayerType currentLayer;

    void Start()
    {
        // Set the GameObject layer using the enum
        //gameObject.layer = (int)LayerType.Gameplay;

        // Get the layer name as a string
        // string layerName = GetLayerName(currentLayer);
        // Debug.Log("Current Layer: " + layerName);
    }

    public void SetLayer(LayerType layer)
    {
        currentLayer = layer;
        gameObject.layer = (int)layer;
        Debug.Log("Layer set to: " + GetLayerName(layer));
    }

    public static string GetLayerName(LayerType layerType)
    {
        // Get the string representation of the enum
        return LayerTypeNames.ContainsKey(layerType) ? LayerTypeNames[layerType] : "Unknown Layer";
    }
}
