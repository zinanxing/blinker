// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainMenu.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;

using UnityEngine;

public class MainMenu : Photon.MonoBehaviour
{
    private string roomName = "myRoom";
	
	private bool isGUION;

    private Vector2 scrollPos = Vector2.zero;

    private bool connectFailed = false;

    private void Awake()
    {
        // Connect to the main photon server. This is the only IP and port we ever need to set(!)
		
		isGUION = true;
		
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        // PhotonNetwork.logLevel = NetworkLogLevel.Full; // turn on if needed

        // Load name from PlayerPrefs
        PhotonNetwork.playerName = PlayerPrefs.GetString("playerName", "Guest" + Random.Range(1, 9999));
    }

    private void OnGUI()
    {
		if(isGUION){
        if (!PhotonNetwork.connected)
        {
            if (PhotonNetwork.connectionState == ConnectionState.Connecting)
            {
                GUILayout.Label("Connecting...");
            }
            else
            {
                GUILayout.Label("Not connected. Check console output.");
            }

            if (this.connectFailed)
            {
                GUILayout.Label("Connection failed. Check setup and use Setup Wizard to fix configuration.");
                GUILayout.Label(string.Format("Server: {0}:{1}", PhotonNetwork.PhotonServerSettings.ServerAddress, PhotonNetwork.PhotonServerSettings.ServerPort));
                GUILayout.Label(string.Format("AppId: {0}", PhotonNetwork.PhotonServerSettings.AppID));
                
                if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                {
                    this.connectFailed = false;
                    PhotonNetwork.ConnectUsingSettings();
                }
            }

            return;
        }

        GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 300) / 2, 400, 300));

        GUILayout.Label("Main Menu");

        // Player name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Player name:", GUILayout.Width(150));
        PhotonNetwork.playerName = GUILayout.TextField(PhotonNetwork.playerName);
        if (GUI.changed)
        {
            // Save name
            PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(15);



        // Create a room (fails if exist!)
        GUILayout.BeginHorizontal();
        GUILayout.Label("CREATE ROOM:", GUILayout.Width(150));
        this.roomName = GUILayout.TextField(this.roomName);
        if (GUILayout.Button("GO"))
        {
            PhotonNetwork.CreateRoom(this.roomName, true, true, 10);
				
			
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(30);
        GUILayout.Label("ROOM LISTING:");
        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            GUILayout.Label("..no games available..");
        }
        else
        {
            // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            foreach (Room game in PhotonNetwork.GetRoomList())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(game.name + " " + game.playerCount + "/" + game.maxPlayers);
                if (GUILayout.Button("JOIN"))
                {
                    PhotonNetwork.JoinRoom(game.name);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();
			
			
			
		}
    }

    // We have two options here: we either joined(by title, list or random) or created a room.
    private void OnJoinedRoom()
    {
		this.StartCoroutine(this.MoveToGameScene());
		isGUION = false;
    }

    private void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        this.StartCoroutine(this.MoveToGameScene());
		isGUION = false;

    }

    private void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    private void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters);
    }

    private IEnumerator MoveToGameScene()
    {
        while (PhotonNetwork.room == null)
        {
            yield return 0;
        }

        // Temporary disable processing of futher network messages
        PhotonNetwork.isMessageQueueRunning = false;
        Application.LoadLevel("GameScene");
    }
}