using UnityEngine;

public class TestInteract : MonoBehaviour, IInteractable
{
    public string GetPrompt(KeyCode interactKey)
    {
        return $"Press {interactKey} to interact with the object";
    }

    public void Interact(Transform fromTransform)
    {
        Debug.Log("Interact");
    }
}
