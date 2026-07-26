namespace ScreenWorking.Collaboration.Editor.Models
{
    public enum SerializedValueType
    {
        Null = 0,
        Integer = 1,
        Long = 2,
        Float = 3,
        Double = 4,
        Boolean = 5,
        String = 6,
        Enum = 7,
        Vector2 = 8,
        Vector3 = 9,
        Vector4 = 10,
        Vector2Int = 11,
        Vector3Int = 12,
        Quaternion = 13,
        Color = 14,
        Color32 = 15,
        Rect = 16,
        RectInt = 17,
        Bounds = 18,
        BoundsInt = 19,
        LayerMask = 20,
        SceneObjectRef = 21,
        AssetRef = 22,
        Array = 23,
        Unsupported = 99
    }
}
