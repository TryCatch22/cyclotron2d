using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Cyclotron2D
{
	interface IDrawerTransform
	{
		Matrix Transform { get; }
		Matrix InvTransform { get; }
	}

	class Camera : IDrawerTransform
	{
		public static readonly IDrawerTransform Identity = new ReadOnlyCamera(new Camera());

		private Vector2 center;
		public Vector2 Center
		{
			get { return center; }
			set
			{
				center = value;
				invalidated = true;
			}
		}

		private Vector2 displacement;
		public Vector2 Displacement
		{
			get { return displacement; }
			set
			{
				displacement = value;
				invalidated = true;
			}
		}

		private Vector2 scale;
		public Vector2 Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				invalidated = true;
			}
		}


		private Matrix transform;
		public Matrix Transform
		{
			get
			{
				if (invalidated)
					ComputeTransform();
				return transform;
			}

			set
			{
				transform = value;
				invTransform = Matrix.Invert(transform);
			}
		}

		private Matrix invTransform;
		public Matrix InvTransform
		{
			get
			{
				if (invalidated)
					ComputeTransform();
				return invTransform;
			}
		}

		public Vector2 ViewSize
		{
			get { return new Vector2(2 * halfScreenSize.X / Scale.X, 2 * halfScreenSize.Y / scale.Y); }
		}

		private bool invalidated = false;
		Vector2 halfScreenSize;

		private Camera()
		{
			invalidated = false;
			transform = Matrix.Identity;
			invTransform = Matrix.Identity;
		}

		public Camera(Vector2 screenSize)
		{
			halfScreenSize = screenSize / 2;
			scale = Vector2.One;
		}

		private void ComputeTransform()
		{
			Vector3 pos = new Vector3(center + displacement * scale, 0);
			transform = Matrix.CreateTranslation(-halfScreenSize.X, -halfScreenSize.Y, 0) * Matrix.CreateScale(scale.X, scale.Y, 1f) *
				Matrix.CreateTranslation(pos);
			invTransform = Matrix.Invert(transform);
			invalidated = false;
		}


		class ReadOnlyCamera : IDrawerTransform
		{
			Camera camera;
			public ReadOnlyCamera(Camera camera)
			{
				this.camera = camera;
			}

			public Matrix Transform { get { return camera.transform; } }
			public Matrix InvTransform { get { return camera.invTransform; } }
		}
	}
}
