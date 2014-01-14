using System;
using System.Collections.Generic;
using Cocos2D;
using XNA = Microsoft.Xna.Framework;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.listener;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;

namespace GoneBananas
{
	public class GameStartLayer : CCLayerColor, ConnectionRequestListener, RoomRequestListener, ZoneRequestListener
	{
		CCLabelTTF statusLabel;
		String status;
		public GameStartLayer ()
		{
			TouchEnabled = true;

			statusLabel = new CCLabelTTF (status, "MarkerFelt", 22) {
				Position = CCDirector.SharedDirector.WinSize.Center,
				Color = new CCColor3B (XNA.Color.Yellow)
			};

			AddChild(statusLabel);

			Color = new CCColor3B (XNA.Color.ForestGreen);
			Opacity = 255;
		}

		public override void OnEnter()
		{
			WarpClient.GetInstance ().AddConnectionRequestListener (this);
			WarpClient.GetInstance ().AddRoomRequestListener (this);
			WarpClient.GetInstance ().AddZoneRequestListener (this);
			Context.isRoomCreator = false; 
			status = "Tap Screen to Go Bananas!";
			base.OnEnter ();
		}

		public override void OnExit()
		{
			WarpClient.GetInstance ().RemoveConnectionRequestListener (this);
			WarpClient.GetInstance ().RemoveRoomRequestListener (this);
			WarpClient.GetInstance ().RemoveZoneRequestListener (this);
			base.OnExit ();
		}

		public override void Visit()
		{
			statusLabel.Text = status;
			base.Visit ();
		}

		public override void TouchesEnded (List<CCTouch> touches)
		{
			base.TouchesEnded (touches);

			byte state = WarpClient.GetInstance ().GetConnectionState ();
			if (state == WarpConnectionState.CONNECTED) {
				status = "joining room..";
				WarpClient.GetInstance ().JoinRoomInRange (1, 1, true);
			} else if(state == WarpConnectionState.DISCONNECTED){
				status = "Connecting..";
				WarpClient.GetInstance ().Connect (Context.username);
			}
		}

		public static CCScene Scene {
			get {
				var scene = new CCScene ();
				var layer = new GameStartLayer ();

				scene.AddChild (layer);

				return scene;
			}
		}

		public void onConnectDone(ConnectEvent eventObj)
		{
			status = "onConnectDone.."+eventObj.getResult ();
			if (eventObj.getResult () == WarpResponseResultCode.SUCCESS) {
				WarpClient.GetInstance ().initUDP ();
				WarpClient.GetInstance ().JoinRoomInRange (1, 1, true);
			}
		}

		public void onDisconnectDone(ConnectEvent eventObj)
		{

		}

		public void onInitUDPDone(byte resultCode)
		{
			status = "onInitUDPDone "+resultCode;
			if (resultCode == WarpResponseResultCode.SUCCESS) {
				Context.udpEnabled = true;
			}
		}

		/**
		 * 
		 */
		public void onSubscribeRoomDone(RoomEvent eventObj)
		{
			status = "onSubscribeRoomDone "+eventObj.getResult ();
			if (eventObj.getResult() == WarpResponseResultCode.SUCCESS)
			{
				CCDirector.SharedDirector.ReplaceScene (GameLayer.Scene);
			}
		}
		public void onUnSubscribeRoomDone(RoomEvent eventObj)
		{
			if (eventObj.getResult() == WarpResponseResultCode.SUCCESS)
			{
				//Deployment.Current.Dispatcher.BeginInvoke(new ShowResultCallback(mShowResultCallback), "Yay! UnSubscribe room");
			}
		}

		public void onJoinRoomDone(RoomEvent eventObj)
		{
			status = "onJoinRoomDone " + eventObj.getResult ();
			if (eventObj.getResult () == WarpResponseResultCode.SUCCESS) {
				Context.gameRoomId = eventObj.getData ().getId ();
				WarpClient.GetInstance ().SubscribeRoom (Context.gameRoomId);
			} else {
				WarpClient.GetInstance ().CreateRoom ("bananas", "monkey", 2, null);
			}
		}

		public void onLeaveRoomDone(RoomEvent eventObj)
		{

		}

		public void onGetLiveRoomInfoDone(LiveRoomInfoEvent eventObj)
		{
			      
		}

		public void onSetCustomRoomDataDone(LiveRoomInfoEvent eventObj)
		{

		}

		public void onUpdatePropertyDone(LiveRoomInfoEvent lifeLiveRoomInfoEvent)
		{
		}
		public void onLockPropertiesDone(byte result)
		{
			//  throw new NotImplementedException();
		}

		public void onUnlockPropertiesDone(byte result)
		{
		}

		public void onCreateRoomDone(com.shephertz.app42.gaming.multiplayer.client.events.RoomEvent eventObj)
		{
			status = "onCreateRoomDone "+eventObj.getResult();
			WarpClient.GetInstance().JoinRoom(eventObj.getData ().getId ());
			Context.isRoomCreator = true;
		}

		public void onDeleteRoomDone(com.shephertz.app42.gaming.multiplayer.client.events.RoomEvent eventObj)
		{
			//throw new NotImplementedException();
		}

		public void onGetAllRoomsDone(com.shephertz.app42.gaming.multiplayer.client.events.AllRoomsEvent eventObj)
		{
			// throw new NotImplementedException();
		}

		public void onGetLiveUserInfoDone(com.shephertz.app42.gaming.multiplayer.client.events.LiveUserInfoEvent eventObj)
		{

		}

		public void onGetMatchedRoomsDone(com.shephertz.app42.gaming.multiplayer.client.events.MatchedRoomsEvent matchedRoomsEvent)
		{
			//throw new NotImplementedException();
		}

		public void onGetOnlineUsersDone(com.shephertz.app42.gaming.multiplayer.client.events.AllUsersEvent eventObj)
		{
			// throw new NotImplementedException();
		}

		public void onSetCustomUserDataDone(com.shephertz.app42.gaming.multiplayer.client.events.LiveUserInfoEvent eventObj)
		{
			//throw new NotImplementedException();
		}

	}
}

