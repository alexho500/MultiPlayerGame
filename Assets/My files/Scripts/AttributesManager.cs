using UnityEngine;
using Unity.Netcode;

// This manages attributes like health for networked game objects.
public class AttributesManager : NetworkBehaviour
{
    // Networked variable for health, health = 100
    public NetworkVariable<int> health = new NetworkVariable<int>(100);

    // Base attack damage, which would be the projectile
    public int attack = 10;

    // ServerRpc method to apply damage to this object.
    [ServerRpc]
    public void TakeDamageServerRpc(int amount)
    {
        health.Value -= amount; // Reduce health by the damage amount.
        CheckDeath(); // Checking if health reduction results in death.
    }

    // Client method to initiate damage on a target
    public void DealDamage(AttributesManager target)
    {
        // Ensure target exists &&  gets called on the server
        if (target != null && IsServer) 
        {
            //Apply damaage 
            target.TakeDamageServerRpc(attack); 
        }
    }

    // Private method to check if the object's health is 0 or below, if is = death.
    private void CheckDeath()
    {
        if (health.Value <= 0)
        {
            Die(); // Trigger death logic if health is depleted.
        }
    }

    // Handles logic for when the character dies.
    private void Die()
    {
        Debug.Log(gameObject.name + " has died."); // Log death message.

        if (IsServer)
        {
            NetworkObject.Despawn(); // Remove the networked object across all clients.
            Destroy(gameObject); // Destroy the object server-side.
        }
    }
}