using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Cyclotron2D.Sounds
{
	static class Sound
	{
		public static Song MenuMusic;

		public static bool menuMusicPlaying = false;

		public static void Initialize()
		{
			MediaPlayer.IsRepeating = true;
		}

		public static void LoadContent(ContentManager content)
		{
			MenuMusic = content.Load<Song>(@"MenuMusic");
		}

	}
}
