using UnityEngine;

public abstract class Interactable : MonoBehaviour
{

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    public abstract void Interact(GameObject interactor);



}
