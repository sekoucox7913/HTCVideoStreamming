#region License
/*
 * TestSocketIO.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 Fabio Panettieri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System.Collections;
using UnityEngine;
using SocketIO;
using Valve.VR;
using RenderHeads.Media.AVProVideo;

public class TestSocketIO : MonoBehaviour
{
	public SteamVR_ControllerManager cameraRig;
	public MediaPlayer mediaPlayer;
	public GameObject leftMsgbox, rightMsgbox;

	[HideInInspector]public string videoUrl= "http://cm-video-streamer.westus.cloudapp.azure.com:8000/video_socket​";

	private SocketIOComponent socket;
	private Transform cameraHead;

	void Start() 
	{
//		mediaPlayer.OpenVideoFromFile (MediaPlayer.FileLocation.AbsolutePathOrURL, videoUrl);
		GameObject go = GameObject.Find("SocketIO");
		socket = go.GetComponent<SocketIOComponent>();

		socket.On("open", TestOpen);
		socket.On("boop", TestBoop);
		socket.On("error", TestError);
		socket.On("close", TestClose);
		cameraHead = cameraRig.GetComponentInChildren<SteamVR_Camera> ().transform;

		InvokeRepeating ("SendTrans", 1, 0.1f);

		socket.On ("video_message", ShowMessage);
	}

	void SendTrans() {
		JSONObject jsonObject = new JSONObject (JSONObject.Type.OBJECT);
		jsonObject.AddField ("leftController", GetTrans (cameraRig.left.transform));
		jsonObject.AddField ("rightController", GetTrans (cameraRig.right.transform));
		jsonObject.AddField ("headset", GetTrans (cameraHead));
		socket.Emit ("update", jsonObject);
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

	public void ShowMessage(SocketIOEvent e) {
		Debug.Log (e.name + "=====================" + e.data);

		if (e.data ["display"].ToString() == "true") {
			if (e.data ["Controller"].ToString().Contains("left")) {
				leftMsgbox.GetComponentInChildren<TextMesh> ().text = e.data ["message"].ToString ();
				leftMsgbox.SetActive (true);
			} else {
				rightMsgbox.GetComponentInChildren<TextMesh> ().text = e.data ["message"].ToString ();
				rightMsgbox.SetActive (true);
			}
		} else {
			if (e.data ["Controller"].ToString().Contains("left"))
				leftMsgbox.SetActive (false);
			else
				rightMsgbox.SetActive (false);
		}
	}

	public void TestOpen(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
	}
	
	public void TestBoop(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Boop received: " + e.name + " " + e.data);

		if (e.data == null) { return; }

		Debug.Log(
			"#####################################################" +
			"THIS: " + e.data.GetField("this").str +
			"#####################################################"
		);
	}
	
	public void TestError(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
	}
	
	public void TestClose(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}
}