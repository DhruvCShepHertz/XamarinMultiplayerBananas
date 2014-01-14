using System;
using Microsoft.Xna.Framework;
using Cocos2D;
using com.shephertz.app42.gaming.multiplayer.client;

namespace GoneBananas
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GoneBananasGame : Game
    {
        readonly GraphicsDeviceManager graphics;

        public GoneBananasGame()
        {
            graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";
            graphics.IsFullScreen = false;

            CCApplication application = new GoneBananasApplication(this, graphics);
            Components.Add(application);

			WarpClient.initialize(Constants.AppKey, Constants.SecretKey);

			// You can take input from user or do facebook/3rd party login etc.
			// Using random name to keep things simple
			Context.username = RandomString(4);
        }

		private static Random random = new Random((int)DateTime.Now.Ticks);

		private string RandomString(int size)
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			char ch;
			for (int i = 0; i < size; i++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));                 
				builder.Append(ch);
			}

			return builder.ToString();
		}
    }
}