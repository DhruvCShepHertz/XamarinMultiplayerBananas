using Microsoft.Xna.Framework;
using Cocos2D;
using CocosDenshion;

namespace GoneBananas
{
	public class GoneBananasApplication : CCApplication
	{
		public GoneBananasApplication(Game game, GraphicsDeviceManager graphics)
			: base(game, graphics)
		{
			s_pSharedApplication = this;

            CCDrawManager.InitializeDisplay(game, 
			                              graphics, 
			                              DisplayOrientation.Portrait);

			graphics.PreferMultiSampling = false;	
		}

		public override bool ApplicationDidFinishLaunching()
		{
			//initialize director
			CCDirector director = CCDirector.SharedDirector;
			director.SetOpenGlView();

			// 2D projection
			director.Projection = CCDirectorProjection.Projection2D;

			// turn on display FPS
			director.DisplayStats = false;

			// set FPS. the default value is 1.0/60 if you don't call this
			director.AnimationInterval = 1.0 / 60;
			
			CCScene scene = GameStartLayer.Scene;

			director.RunWithScene(scene);

			// returning true indicates the app initialized successfully and can continue
			return true;
		}

		public override void ApplicationDidEnterBackground()
		{
            // stop all of the animation actions that are running.
			CCDirector.SharedDirector.Pause();
			
			// if you use SimpleAudioEngine, your music must be paused
			//CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic = true;
		}

		public override void ApplicationWillEnterForeground()
		{
            CCDirector.SharedDirector.Resume();
			
			// if you use SimpleAudioEngine, your background music track must resume here. 
			//CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic = false;
		}
	}
}