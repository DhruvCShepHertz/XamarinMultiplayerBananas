using System;
using System.Collections.Generic;
using Cocos2D;
using CocosDenshion;
using com.shephertz.app42.gaming.multiplayer.client.listener;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using XNA = Microsoft.Xna.Framework;
using System.IO;

namespace GoneBananas
{
	public class GameLayer : CCLayerColor, NotifyListener
	{
		const float MONKEY_SPEED = 500.0f;
		float dt = 0;
		CCSprite localMonkey;
		CCSprite remoteMonkey;
		List<CCSprite> localBananas;
		int localHitBananas = 0;
		int remoteHitBananas = 0;
		public override void OnEnter()
		{
			WarpClient.GetInstance ().AddNotificationListener (this);
			base.OnEnter ();
		}

		public override void OnExit()
		{
			WarpClient.GetInstance ().RemoveNotificationListener (this);
			WarpClient.GetInstance ().LeaveRoom (Context.gameRoomId);
			WarpClient.GetInstance ().UnsubscribeRoom (Context.gameRoomId);
			base.OnExit ();
		}

		public GameLayer ()
		{
			TouchEnabled = true;


			localBananas = new List<CCSprite> ();
	
			localMonkey = new CCSprite ("MonkeyLocal");

			localMonkey.PositionY = 0;
			localMonkey.PositionX = CCDirector.SharedDirector.WinSize.Width / 2;
			AddChild (localMonkey);


			remoteMonkey = new CCSprite ("MonkeyRemote");
			remoteMonkey.PositionY = CCDirector.SharedDirector.WinSize.Height - remoteMonkey.ContentSizeInPixels.Height/2;
			remoteMonkey.PositionX = CCDirector.SharedDirector.WinSize.Width / 2;
			AddChild (remoteMonkey);

			if (!Context.isRoomCreator) {
				byte[] message = buildStartMessage ();
				sendWarpUpdate (message);
				beginLocalGame ();
			}

			Color = new CCColor3B (XNA.Color.ForestGreen);
			Opacity = 255;
		}

		private void sendWarpUpdate(byte[] message)
		{
			if(Context.udpEnabled){
				WarpClient.GetInstance ().SendUDPUpdatePeers (message);
			}
			else{
				WarpClient.GetInstance ().SendUpdatePeers (message);
			}
		}

		private void beginLocalGame()
		{
			Console.WriteLine("beginLocalGame");
			Schedule ((t) => {
				localBananas.Add (AddLocalBanana ());
				dt += t;
				if(ShouldEndGame ()){
					dt = 0;
					var gameOverScene = GameOverLayer.SceneWithScore(localHitBananas, remoteHitBananas);
					CCDirector.SharedDirector.ReplaceScene (gameOverScene);
					localHitBananas = 0;
				}
			}, 2.0f);

			Schedule ((t) => {
				CheckCollision ();
			});
		}

		CCSprite AddLocalBanana ()
		{
			var banana = new CCSprite ("BananaLocal");
			banana.Tag = 0;
			double rnd = new Random ().NextDouble ();
			double randomX = (rnd > 0) 
				? rnd * CCDirector.SharedDirector.WinSize.Width - banana.ContentSize.Width / 2 
				: banana.ContentSize.Width / 2;
	
			banana.Position = new CCPoint ((float)randomX, CCDirector.SharedDirector.WinSize.Height - banana.ContentSize.Height / 2);

			byte[] message = buildBananaMessage ((float)randomX);
			sendWarpUpdate (message);

			AddChild (banana);

			var moveBanana = new CCMoveTo (5.0f, new CCPoint (banana.Position.X, 0));

			var moveBananaComplete = new CCCallFuncN ((node) => {
				node.RemoveFromParentAndCleanup (true);
				localBananas.Remove(banana);
			});

			var moveBananaSequence = new CCSequence (moveBanana, moveBananaComplete);

			banana.RunAction (moveBananaSequence);

			return banana;
		}

		void CheckCollision ()
		{
			localBananas.ForEach ((banana) => {
				bool hit = banana.BoundingBox.IntersectsRect (localMonkey.BoundingBox);
				if (hit && (banana.Tag!=1)) {
					banana.Tag = 1;
					localHitBananas++;
					CCSimpleAudioEngine.SharedEngine.PlayEffect ("Sounds/localhit.mp3");
					byte[] message = buildHitMessage(localHitBananas);
					sendWarpUpdate(message);
				}
			});
		}

		bool ShouldEndGame()
		{
			return dt > 20.0;
		}

		public override void TouchesEnded (List<CCTouch> touches)
		{
			base.TouchesEnded (touches);

			var location = new CCPoint ();
			location.X = touches [0].Location.X;
			location.Y = localMonkey.PositionY;
			float ds = CCPoint.Distance (localMonkey.Position, location);

			float dt = ds / MONKEY_SPEED;

			var moveMonkey = new CCMoveTo (dt, location);
			localMonkey.RunAction (moveMonkey);	

			byte[] message = buildMonkeyMessage (location.X);
			sendWarpUpdate (message);
		}

		private byte[] buildBananaMessage(float x)
		{

			char[] nameChars = Context.username.ToCharArray ();

			MemoryStream memStream = new MemoryStream();
			BinaryWriter buf = new BinaryWriter(memStream);
			memStream.SetLength(5 + nameChars.Length*sizeof(char));

			buf.Write ((byte)MessageType.banana);
			buf.Write (x);
			buf.Write (nameChars);

			byte[] message = new byte[memStream.Length];

			Buffer.BlockCopy (memStream.GetBuffer (), 0, message, 0, (int)memStream.Length);
			Console.WriteLine ("buildBananaMessage bytes "+message.Length);
			return message;
		}

		private byte[] buildHitMessage(float count)
		{
			char[] nameChars = Context.username.ToCharArray ();

			MemoryStream memStream = new MemoryStream();
			BinaryWriter buf = new BinaryWriter(memStream);
			memStream.SetLength(5 + nameChars.Length*sizeof(char));

			buf.Write ((byte)MessageType.hitcount);
			buf.Write (count);
			buf.Write (nameChars);

			byte[] message = new byte[memStream.Length];

			Buffer.BlockCopy (memStream.GetBuffer (), 0, message, 0, (int)memStream.Length);
			Console.WriteLine ("buildHitMessage bytes "+message.Length);
			return message;
		}

		private byte[] buildStartMessage()
		{
			char[] nameChars = Context.username.ToCharArray ();

			MemoryStream memStream = new MemoryStream();
			BinaryWriter buf = new BinaryWriter(memStream);
			memStream.SetLength(1 + nameChars.Length*sizeof(char));

			buf.Write ((byte)MessageType.start);
			buf.Write ((float)0);
			buf.Write (nameChars);

			byte[] message = new byte[memStream.Length];

			Buffer.BlockCopy (memStream.GetBuffer (), 0, message, 0, (int)memStream.Length);
			Console.WriteLine ("buildBananaMessage bytes "+message.Length);
			return message;
		}

		private byte[] buildMonkeyMessage(float x)
		{

			char[] nameChars = Context.username.ToCharArray ();

			MemoryStream memStream = new MemoryStream();
			BinaryWriter buf = new BinaryWriter(memStream);
			memStream.SetLength(5 + nameChars.Length*sizeof(char));

			buf.Write ((byte)MessageType.monkey);
			buf.Write (x);
			buf.Write (nameChars);

			byte[] message = new byte[memStream.Length];

			Buffer.BlockCopy (memStream.GetBuffer (), 0, message, 0, (int)memStream.Length);
			Console.WriteLine ("buildMonkeyMessage bytes "+message.Length);
			return message;
		}

		public static CCScene Scene {
			get {
				var scene = new CCScene ();
				var layer = new GameLayer ();
			
				scene.AddChild (layer);

				return scene;
			}
		}

		public void onRoomCreated(RoomData eventObj)
		{

		}

		public void onRoomDestroyed(RoomData eventObj)
		{

		}

		public void onUserLeftRoom(RoomData eventObj, String username)
		{

		}

		public void onUserJoinedRoom(RoomData eventObj, String username)
		{

		}

		public void onUserLeftLobby(LobbyData eventObj, String username)
		{

		}

		public void onUserJoinedLobby(LobbyData eventObj, String username)
		{

		}

		public void onChatReceived(ChatEvent eventObj)
		{

		}

		public void onUpdatePeersReceived(UpdateEvent eventObj)
		{
			Console.WriteLine ("onUpdatePeersReceived "+eventObj.getUpdate().Length+" bytes isUDP "+eventObj.getIsUdp());

			byte[] update = eventObj.getUpdate ();
			int updateLen = update.Length;
			BinaryReader buf = new BinaryReader(new MemoryStream(update));


			MessageType type = (MessageType)buf.ReadByte();
			float data = buf.ReadSingle ();
			char[] nameChars = buf.ReadChars (updateLen-5);
			String senderName = new String (nameChars);

			Console.WriteLine ("type: "+type+" data: "+data+" sender: "+senderName);
			if (senderName.Contains(Context.username)) {
				Console.WriteLine ("Ignoring self update");
				return;
			}
			if (type == MessageType.monkey) {
				moveRemoteMonkey (data);
			} else if (type == MessageType.banana) {
				addRemoteBanana (data);
			} else if (type == MessageType.hitcount) {
				remoteHitBananas = (int)data;
				CCSimpleAudioEngine.SharedEngine.PlayEffect ("Sounds/remotehit.mp3");
			}
			else if(type == MessageType.start){
				beginLocalGame ();
			}
		}

		private void addRemoteBanana(float x)
		{
			Console.WriteLine ("addRemoteBanana");
			var banana = new CCSprite ("BananaRemote");

			banana.Position = new CCPoint (x, banana.ContentSize.Height / 2);


			AddChild (banana);

			var moveBanana = new CCMoveTo (5.0f, new CCPoint (banana.Position.X, CCDirector.SharedDirector.WinSize.Height));

			var moveBananaComplete = new CCCallFuncN ((node) => {
				node.RemoveFromParentAndCleanup (true);
			});

			var moveBananaSequence = new CCSequence (moveBanana, moveBananaComplete);

			banana.RunAction (moveBananaSequence);
		}

		private void moveRemoteMonkey(float x)
		{
			Console.WriteLine ("moveRemoteMonkey");
			var location = new CCPoint ();
			location.X = x;
			location.Y = remoteMonkey.PositionY;
			float ds = CCPoint.Distance (remoteMonkey.Position, location);

			float dt = ds / MONKEY_SPEED;

			var moveMonkey = new CCMoveTo (dt, location);
			remoteMonkey.RunAction (moveMonkey);	
		}

		public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<string, object> properties)
		{

		}
		public void onMoveCompleted(MoveEvent moveEvent)
		{

		}

		public void onPrivateChatReceived(string sender, string message)
		{
			throw new NotImplementedException();
		}

		public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<string, object> properties, Dictionary<string, string> lockedPropertiesTable)
		{

		}


		public void onUserPaused(string locid, bool isLobby, string username)
		{

		}

		public void onUserResumed(string locid, bool isLobby, string username)
		{

		}


		public void onGameStarted(string sender, string roomId, string nextTurn)
		{
			  
		}

		public void onGameStopped(string sender, string roomId)
		{
			//throw new NotImplementedException();
		}
	}
}

