using System.Collections.Generic;
using UnityEngine;

public class ThreatDetector : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f; // detection for parties to chase / flee
    private BaseNPCStateMachine npc;
    private PartyPresence self;
    private CircleCollider2D detectionCollider;
    [SerializeField] private LayerMask detectionLayerMask;

    void Awake()
    {
        npc = GetComponentInParent<BaseNPCStateMachine>();
        self = GetComponentInParent<PartyPresence>();

        detectionCollider = GetComponent<CircleCollider2D>();

        if (detectionCollider != null)
        {
            detectionCollider.isTrigger = true;
            detectionCollider.radius = detectionRange;
        }
        else
        {
            Debug.LogError("ThreatDetector requires a CircleCollider2D.");
        }
    }

    void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((detectionLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.gameObject == self.gameObject) return;
        Debug.Log("Enter");
        PartyPresence otherPresence = other.GetComponent<PartyPresence>();
        if (otherPresence == null) return;

        if (FactionsManager.Instance.AreEnemies(self.Lord.Faction, otherPresence.Lord.Faction))
            npc.OnThreatDetected(otherPresence);
        else if (FactionsManager.Instance.AreAllied(self.Lord.Faction, otherPresence.Lord.Faction))
            npc.OnFriendDetected(otherPresence);
    }


    private Dictionary<PartyPresence, string> relationshipMap = new();

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject == self.gameObject) return;

        PartyPresence otherPresence = other.GetComponent<PartyPresence>();
        if (otherPresence == null) return;

        if ((detectionLayerMask.value & (1 << other.gameObject.layer)) == 0)
        {
            npc.OnThreatExited(otherPresence);
            npc.OnFriendExited(otherPresence);
            return;
        }

        string newRelation;
        if (FactionsManager.Instance.AreEnemies(self.Lord.Faction, otherPresence.Lord.Faction))
            newRelation = "Enemy";
        else if (FactionsManager.Instance.AreAllied(self.Lord.Faction, otherPresence.Lord.Faction))
            newRelation = "Ally";
        else
            newRelation = "Neutral";

        if (!relationshipMap.TryGetValue(otherPresence, out var oldRelation) || oldRelation != newRelation)
        {
            // relationship changed → update lists
            if (oldRelation == "Enemy") npc.OnThreatExited(otherPresence);
            if (oldRelation == "Ally") npc.OnFriendExited(otherPresence);

            if (newRelation == "Enemy") npc.OnThreatDetected(otherPresence);
            if (newRelation == "Ally") npc.OnFriendDetected(otherPresence);

            relationshipMap[otherPresence] = newRelation;
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        PartyPresence otherPresence = other.GetComponent<PartyPresence>();
        if (otherPresence == null) return;

        if (relationshipMap.TryGetValue(otherPresence, out var oldRelation))
        {
            if (oldRelation == "Enemy") npc.OnThreatExited(otherPresence);
            if (oldRelation == "Ally") npc.OnFriendExited(otherPresence);
            relationshipMap.Remove(otherPresence);
        }
    }
}
