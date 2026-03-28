using UnityEngine;

public interface IInteractable
{
    string GetPrompt(KeyCode interactKey);
    void Interact(Transform fromTransform);
}
