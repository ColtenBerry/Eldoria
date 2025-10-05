using System.Collections.Generic;
using System.Numerics;

public interface IInteractable
{
    void Interact();
    List<InteractionOption> GetInteractionOptions();
}