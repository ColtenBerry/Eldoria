using UnityEngine;


// This exists just so i can handle clicking on a card. Unit, inventory item, etc. Honestly kind of a pain but this
// is the best solution I can think up. 
public interface ICardHandler
{
    void OnCardClicked(UnitInstance unit);
}
