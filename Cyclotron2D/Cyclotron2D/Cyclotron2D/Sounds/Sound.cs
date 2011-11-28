using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Cyclotron2D.Mod;

namespace Cyclotron2D.Sounds
{
	static class Sound
	{
		public static Song MenuMusic;

		public static SoundEffect Boom, BlipLow, BlipHigh;

		public bool Mute { get { return Settings.SinglePlayer.Mute.Value; } }

		public static bool menuMusicPlaying = false;

		public static void Initialize()
		{
			MediaPlayer.IsRepeating = true;
		}

		public static void LoadContent(ContentManager content)
		{
			Boom = content.Load<SoundEffect>(@"Boom");
			BlipLow = content.Load<SoundEffect>(@"BlipLow");
			BlipHigh = content.Load<SoundEffect>(@"BlipHigh");

			MenuMusic = content.Load<Song>(@"MenuMusic");
		}

	}
}
