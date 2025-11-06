using UnityEngine;

public interface ISiegable
{
    void StartSiege(PartyController attacker, bool isPlayer);

    bool isUnderSiege { get; }

}
