using UnityEngine;

// Any GameObject that can participate in gameplay logic should implement this interface
public interface IInteractable {

    // interact with a Interactable without any context
    // returns true if interaction occured
    bool Interact();

    // interact with a Interactable with any context
    // returns true if interaction occured
    bool InteractWith(IInteractable other);

}
