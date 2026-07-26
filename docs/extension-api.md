# [screen working] Extensibility API Guide

## Extending Adapters & Serializers
Implement `ICollaborationAdapter` or custom property serialization logic to support third-party components (Cinemachine, ProBuilder, custom level design tools):

```csharp
public interface ICollaborationAdapter
{
    string TargetComponentType { get; }
    SerializedValue SerializeComponent(Component component);
    void DeserializeComponent(Component component, SerializedValue payload);
}
```
