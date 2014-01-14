using System;
using System.Collections.Generic;
using Cocos2D;
using XNA = Microsoft.Xna.Framework;
using com.shephertz.app42.gaming.multiplayer.client;

namespace GoneBananas
{
	public class GameOverLayer : CCLayerColor
	{
		public GameOverLayer (int score, int remoteScore)
		{
			TouchEnabled = true;

			string scoreMessage = String.Format ("Game Over. You collected {0} bananas \r\n Opponent collected {1} bananas!", score, remoteScore);

			var scoreLabel = new CCLabelTTF (scoreMessage, "MarkerFelt", 22) {
				Position = new CCPoint( CCDirector.SharedDirector.WinSize.Center.X,  CCDirector.SharedDirector.WinSize.Center.Y + 50),
				Color = new CCColor3B (XNA.Color.Yellow)
			};

			AddChild (scoreLabel);

			var playAgainLabel = new CCLabelTTF ("Tap to Play Again", "MarkerFelt", 22) {
				Position = CCDirector.SharedDirector.WinSize.Center,
				Color = new CCColor3B (XNA.Color.Green)
			};

			AddChild (playAgainLabel);

			Color = new CCColor3B (XNA.Color.Black);
			Opacity = 255;
			// Clean up the room from the server
			if (Context.isRoomCreator) {
				WarpClient.GetInstance ().DeleteRoom (Context.gameRoomId);
			}
		}

		public override void TouchesEnded (List<CCTouch> touches)
		{
			base.TouchesEnded (touches);

			CCDirector.SharedDirector.ReplaceScene (GameStartLayer.Scene);
		}

		public static CCScene SceneWithScore (int score, int remoteScore)
		{
			var scene = new CCScene ();
			var layer = new GameOverLayer (score, remoteScore);

			scene.AddChild (layer);

			return scene;
		}
	}
}

