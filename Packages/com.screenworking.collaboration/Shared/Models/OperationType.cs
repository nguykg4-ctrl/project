namespace ScreenWorking.Collaboration.Editor.Models
{
    public enum OperationType
    {
        CreateGameObject = 1,
        DestroyGameObject = 2,
        RestoreGameObject = 3,
        DuplicateGameObject = 4,
        RenameGameObject = 5,
        SetGameObjectActive = 6,
        SetTagLayer = 7,
        SetStaticFlags = 8,
        ReparentGameObject = 9,
        ReorderSibling = 10,
        AddComponent = 11,
        RemoveComponent = 12,
        SetComponentEnabled = 13,
        ModifyProperty = 14,
        TransformPreview = 15,
        AcquireLock = 16,
        ReleaseLock = 17,
        PresenceUpdate = 18
    }
}
