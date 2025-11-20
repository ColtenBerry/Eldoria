using UnityEngine;

public interface ISiegable
{
    bool IsUnderSiege { get; }
    void StartSiege(PartyController attacker, bool isPlayer);
    void EndSiege();
    void HandleTick(int tick);
    int GetSiegeTicks();


}
