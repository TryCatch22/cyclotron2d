using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Cyclotron2D.Mod;

namespace Cyclotron2D.Sounds
{
	static class Sound
	{
		public static Song MenuMusic;

		public static SoundEffect Boom, BlipLow, BlipHigh, Clink;

		public static bool menuMusicPlaying = false;

		public static void Initialize()
		{
			MediaPlayer.IsRepeating = true;
		}

		public static void PlaySound(SoundEffect sound, float volume)
		{
			if (!MediaPlayer.IsMuted)
			{
				sound.Play(volume, 0.0f, 0.0f);
			}
		}

		public static void LoadContent(ContentManager content)
		{
			Boom = content.Load<SoundEffect>(@"Boom");
			BlipLow = content.Load<SoundEffect>(@"BlipLow");
			BlipHigh = content.Load<SoundEffect>(@"BlipHigh");
			Clink = content.Load<SoundEffect>(@"clink");

			MenuMusic = content.Load<Song>(@"MenuMusic");
		}

	}
}
