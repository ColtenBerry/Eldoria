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


    // TODO: this could be optimized. waiting to see if performance is an issue
    void OnTriggerStay2D(Collider2D other)
    {
        if ((detectionLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            PartyPresence otherPresence = other.GetComponent<PartyPresence>();
            if (otherPresence == null) return;
            if (FactionsManager.Instance.AreEnemies(self.Lord.Faction, otherPresence.Lord.Faction))
                npc.OnThreatDetected(otherPresence);
            else if (FactionsManager.Instance.AreAllied(self.Lord.Faction, otherPresence.Lord.Faction))
                npc.OnFriendDetected(otherPresence);
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == self.gameObject) return;
        if ((detectionLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        Debug.Log("Exit");
        PartyPresence otherPresence = other.GetComponent<PartyPresence>();
        if (otherPresence == null) return;

        if (FactionsManager.Instance.AreEnemies(self.Lord.Faction, otherPresence.Lord.Faction))
            npc.OnThreatExited(otherPresence);
        else if (FactionsManager.Instance.AreAllied(self.Lord.Faction, otherPresence.Lord.Faction))
            npc.OnFriendExited(otherPresence);
    }
}
