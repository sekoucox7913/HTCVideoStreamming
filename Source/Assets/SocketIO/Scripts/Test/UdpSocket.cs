using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RenderHeads.Media.AVProVideo;

public class UdpSocket : MonoBehaviour {
	public static UdpSocket Instance;

	public SteamVR_ControllerManager cameraRig;
	public UdpClient client;
	public IPAddress serverIp;
	public IPEndPoint hostEndPoint;
	public MediaPlayer mediaPlayer;
	public GameObject leftMsgbox, rightMsgbox;
	[HideInInspector]public string videoUrl= "http://localhost:4000/video";

	UTF8Encoding encoding = new UTF8Encoding ();
	Thread networkThread;
	Transform cameraHead;
	string message = "";

	string hostIp = "127.0.0.1";
	int hostPort = 9082;

	// Use this for initialization
	void Start () {
		Instance = this;
		mediaPlayer.OpenVideoFromFile (MediaPlayer.FileLocation.AbsolutePathOrURL, videoUrl);
		serverIp = IPAddress.Parse(hostIp);
		hostEndPoint = new IPEndPoint(serverIp, hostPort);

		client = new UdpClient();
		client.Connect(hostEndPoint);
		//client.Client.Blocking = false;
		networkThread = new Thread (new ThreadStart (UDPThread));
		networkThread.IsBackground = true;
		networkThread.Start ();

		cameraHead = cameraRig.GetComponentInChildren<SteamVR_Camera> ().transform;
		InvokeRepeating ("SendTrans", 1, 0.1f);
	}

	void SendTrans() {
		JSONObject jsonObject = new JSONObject (JSONObject.Type.OBJECT);
		jsonObject.AddField ("leftController", GetTrans (cameraRig.left.transform));
		jsonObject.AddField ("rightController", GetTrans (cameraRig.right.transform));
		jsonObject.AddField ("headset", GetTrans (cameraHead));
		jsonObject.AddField ("message", "transform");
		UDPSend (jsonObject.ToString());
	}

	JSONObject GetTrans(Transform trans) {
		JSONObject json = new JSONObject (JSONObject.Type.OBJECT);
		json.AddField ("x", trans.position.x);
		json.AddField ("y", trans.position.y);
		json.AddField ("z", trans.position.z);
		json.AddField ("tx", trans.eulerAngles.x);
		json.AddField ("ty", trans.eulerAngles.y);
		json.AddField ("tz", trans.eulerAngles.z);
		return json;
	}

	void UDPThread () {
		while (true) {
			byte[] buffer = client.Receive (ref hostEndPoint);
			UDPReceive (buffer);
		}
	}

	void UDPReceive(byte[] buffer){
		message = encoding.GetString (buffer);
	}

	void Update() {
		if (message != "") {
			string[] strArray = message.Split (',');

			if (strArray [0] == "show") {
				if (strArray [1] == "left") {
					leftMsgbox.GetComponentInChildren<TextMesh> ().text = strArray [2];
					leftMsgbox.SetActive (true);
				} else {
					rightMsgbox.GetComponentInChildren<TextMesh> ().text = strArray [2];
					rightMsgbox.SetActive (true);
				}
			} else {
				if (strArray [1] == "left")
					leftMsgbox.SetActive (false);
				else
					rightMsgbox.SetActive (false);
			}

			message = "";
		}
	}

	public void UDPSend (params string[] values){
		byte[] request = encoding.GetBytes (values[0]);
		client.Send (request, request.Length);
	}

	void OnApplicationQuit () {
		networkThread.Abort ();
		client.Close ();
	}
}