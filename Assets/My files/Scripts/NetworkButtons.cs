using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;


//UI buttons for network connections 
public class NetworkButtons : MonoBehaviour {
    private void OnGUI() {
        //Where an area on the screen for the GUI controls
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        // Checking if the application is neither a client nor a server.
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            //These if loops create the desnated button for the role
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        }

        GUILayout.EndArea();
    }

}