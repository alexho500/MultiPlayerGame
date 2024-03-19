using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;


//Player tag name on top
public class PlayerSetting : NetworkBehaviour
{
    //References to players mesh and name UI element 
    [SerializeField] private MeshRenderer meshRenderer; 
    [SerializeField] private TextMeshProUGUI playerName;
    //Player name with default values but only server can write the names 
    private NetworkVariable<FixedString128Bytes> networkPlayerName = new NetworkVariable<FixedString128Bytes>(new FixedString128Bytes("Player: 0"), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // List of possible colors for the player
    public List<Color> colors = new List<Color>();

    // Initialize meshRenderer on awake.
    public void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>(); 
    }

     // When the player spawns on the network, set its player name and color.
    public override void OnNetworkSpawn()
    {
        networkPlayerName.Value = "Player: " + (OwnerClientId + 1);
        playerName.text = networkPlayerName.Value.ToString();
        // Added this operation to prevent IndexOutOfRangeException
        meshRenderer.material.color = colors[(int)OwnerClientId % colors.Count]; 
    }
}
