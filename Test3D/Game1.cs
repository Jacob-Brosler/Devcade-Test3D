using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Devcade;
using System.Collections.Generic;
using System;

// MAKE SURE YOU RENAME ALL PROJECT FILES FROM DevcadeGame TO YOUR YOUR GAME NAME
namespace Test3D
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		
		/// <summary>
		/// Stores the window dimensions in a rectangle object for easy use
		/// </summary>
		private Rectangle windowSize;

        private VertexBuffer VertexBuffer { get; set; }
        private IndexBuffer IndexBuffer { get; set; }
        private BasicEffect effect;

        private List<VertexPositionColor> vertices = new List<VertexPositionColor>();
        private List<short> indices = new List<short>();

        private WrappingInt sideCount = new WrappingInt(8, 3, 20);
        private WrappingInt subDivCount = new WrappingInt(5, 1, 20);
		private WrappingInt curve = new WrappingInt(2, 0, 20);
		private WrappingInt xSize = new WrappingInt(1, 0, 20);
		private WrappingInt ySize = new WrappingInt(1, 0, 20);
		private WrappingInt zSize = new WrappingInt(4, 1, 20);

        private WrappingInt viewHorAngle = new WrappingInt(0, 0, 360);
        private int viewVertAngle = 0; //Limit -60 to 60
        private int viewDist = -10; //Limit -5 to -30

        /// <summary>
        /// Game constructor
        /// </summary>
        public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = false;
		}

		/// <summary>
		/// Performs any setup that doesn't require loaded content before the first frame.
		/// </summary>
		protected override void Initialize()
		{
			// Sets up the input library
			Input.Initialize();

			// Set window size if running debug (in release it will be fullscreen)
			#region
#if DEBUG
			_graphics.PreferredBackBufferWidth = 420;
			_graphics.PreferredBackBufferHeight = 980;
			_graphics.ApplyChanges();
#else
			_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			_graphics.ApplyChanges();
#endif
			#endregion
			
			// TODO: Add your initialization logic here

			windowSize = GraphicsDevice.Viewport.Bounds;

            base.Initialize();
		}

		/// <summary>
		/// Performs any setup that requires loaded content before the first frame.
		/// </summary>
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            // ex:
            // texture = Content.Load<Texture2D>("fileNameWithoutExtension");

            System.Console.WriteLine(GraphicsDevice.RasterizerState);
            effect = new BasicEffect(GraphicsDevice);
            effect.World = Matrix.CreateTranslation(Vector3.Zero);
            effect.View = Matrix.CreateLookAt(new Vector3(0, 0, -10), Vector3.Zero, Vector3.UnitY);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                Window.ClientBounds.Width / (float)Window.ClientBounds.Height,
                1,
                100);
            effect.VertexColorEnabled = true;

            RegenerateMesh();
        }

		/// <summary>
		/// Your main update loop. This runs once every frame, over and over.
		/// </summary>
		/// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
		protected override void Update(GameTime gameTime)
		{
			Input.Update(); // Updates the state of the input library

			// Exit when both menu buttons are pressed (or escape for keyboard debugging)
			// You can change this but it is suggested to keep the keybind of both menu
			// buttons at once for a graceful exit.
			if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
				(Input.GetButton(1, Input.ArcadeButtons.Menu) &&
				Input.GetButton(2, Input.ArcadeButtons.Menu)))
			{
				Exit();
			}

			// TODO: Add your update logic here

			bool camDirty = false;
			bool meshDirty = false;

			if (Input.GetButtonDown(1, Input.ArcadeButtons.StickLeft) || Keyboard.GetState().IsKeyDown(Keys.A))
			{
				viewHorAngle.Add(5);
                camDirty = true;
			}else if (Input.GetButtonDown(1, Input.ArcadeButtons.StickRight) || Keyboard.GetState().IsKeyDown(Keys.D))
            {
                viewHorAngle.Add(-5);
                camDirty = true;
            }

            if (Input.GetButtonDown(1, Input.ArcadeButtons.StickUp) || Keyboard.GetState().IsKeyDown(Keys.W))
            {
                viewVertAngle = Math.Min(60, viewVertAngle + 5);
                camDirty = true;
            }
            else if (Input.GetButtonDown(1, Input.ArcadeButtons.StickDown) || Keyboard.GetState().IsKeyDown(Keys.S))
            {
                viewVertAngle = Math.Max(-60, viewVertAngle - 5);
                camDirty = true;
            }

            if (Input.GetButtonDown(1, Input.ArcadeButtons.A1) || Keyboard.GetState().IsKeyDown(Keys.E))
            {
                viewDist = Math.Min(-5, viewDist + 1);
                camDirty = true;
            }
            else if (Input.GetButtonDown(1, Input.ArcadeButtons.B1) || Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                viewDist = Math.Max(-30, viewDist - 1);
                camDirty = true;
            }

            if (Input.GetButtonDown(1, Input.ArcadeButtons.A3) || Keyboard.GetState().IsKeyDown(Keys.R))
            {
                sideCount.Add(1);
                meshDirty = true;
            }
            else if (Input.GetButtonDown(1, Input.ArcadeButtons.B3) || Keyboard.GetState().IsKeyDown(Keys.F))
            {
                sideCount.Add(-1);
                meshDirty = true;
            }

            if (Input.GetButtonDown(1, Input.ArcadeButtons.A4) || Keyboard.GetState().IsKeyDown(Keys.T))
            {
                subDivCount.Add(1);
                meshDirty = true;
            }
            else if (Input.GetButtonDown(1, Input.ArcadeButtons.B4) || Keyboard.GetState().IsKeyDown(Keys.G))
            {
                subDivCount.Add(-1);
                meshDirty = true;
            }

            if (Input.GetButtonDown(2, Input.ArcadeButtons.A1) || Keyboard.GetState().IsKeyDown(Keys.Y))
            {
                curve.Add(1);
                meshDirty = true;
            }
            else if (Input.GetButtonDown(2, Input.ArcadeButtons.B1) || Keyboard.GetState().IsKeyDown(Keys.H))
            {
                curve.Add(-1);
                meshDirty = true;
            }

            if (Input.GetButtonDown(2, Input.ArcadeButtons.A2) || Keyboard.GetState().IsKeyDown(Keys.U))
            {
                xSize.Add(1);
                meshDirty = true;
            }
            else if (Input.GetButtonDown(2, Input.ArcadeButtons.B2) || Keyboard.GetState().IsKeyDown(Keys.J))
            {
                xSize.Add(-1);
                meshDirty = true;
            }

            if (Input.GetButtonDown(2, Input.ArcadeButtons.A3) || Keyboard.GetState().IsKeyDown(Keys.I))
            {
                ySize.Add(1);
                meshDirty = true;
            }
            else if (Input.GetButtonDown(2, Input.ArcadeButtons.B3) || Keyboard.GetState().IsKeyDown(Keys.K))
            {
                ySize.Add(-1);
                meshDirty = true;
            }

            if (Input.GetButtonDown(2, Input.ArcadeButtons.A4) || Keyboard.GetState().IsKeyDown(Keys.O))
            {
                zSize.Add(1);
                meshDirty = true;
            }
            else if (Input.GetButtonDown(2, Input.ArcadeButtons.B4) || Keyboard.GetState().IsKeyDown(Keys.L))
            {
                zSize.Add(-1);
                meshDirty = true;
            }

            if (camDirty)
            {
                effect.World = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationY((float)((Math.PI / 180) * viewHorAngle.Value)) * Matrix.CreateRotationX((float)((Math.PI / 180) * viewVertAngle));
                effect.View = Matrix.CreateLookAt(new Vector3(0, 0, viewDist), Vector3.Zero, Vector3.UnitY);
            }
            if (meshDirty) RegenerateMesh();

            base.Update(gameTime);
		}

		/// <summary>
		/// Your main draw loop. This runs once every frame, over and over.
		/// </summary>
		/// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the shape
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
            }

            base.Draw(gameTime);
		}

		private void RegenerateMesh()
		{
			vertices.Clear();
			indices.Clear();

			//Start Cap
            vertices.Add(new VertexPositionColor( new Vector3(0,  0, -zSize.Value), Color.Red));
            for (int s = 0; s < sideCount.Value - 1; s++)
            {
                indices.Add(0); indices.Add((short)(s + 1)); indices.Add((short)(s + 2));
            }
            indices.Add(0); indices.Add((short)sideCount.Value); indices.Add(1);

			//Body
            for (int d = 0; d < subDivCount.Value + 1; d++)
			{
				for (int s = 0; s < sideCount.Value; s++)
				{
                    float z = 2 * zSize.Value * (1.0f / subDivCount.Value) * d - zSize.Value;
					vertices.Add(new VertexPositionColor(
						new Vector3(
							(float)(Math.Cos((2 * Math.PI / sideCount.Value) * s) * (xSize.Value + Math.Cos(Math.PI * (z / zSize.Value) / 2) * curve.Value)),
                            (float)(Math.Sin((2 * Math.PI / sideCount.Value) * s) * (ySize.Value + Math.Cos(Math.PI * (z / zSize.Value) / 2) * curve.Value)), 
							z), 
						new Color(s * 32, d * 256, 0)));
					if(d < subDivCount.Value && s != sideCount.Value - 1)
                    {
						short i = (short)(s + d * sideCount.Value + 1);
                        indices.Add(i); indices.Add((short)(i + sideCount.Value)); indices.Add((short)(i + sideCount.Value + 1));
                        indices.Add(i); indices.Add((short)(i + sideCount.Value + 1)); indices.Add((short)(i + 1));
                    }
                }
				if (d < subDivCount.Value)
				{
					short i2 = (short)((d + 1) * sideCount.Value);
					indices.Add(i2); indices.Add((short)(i2 + sideCount.Value)); indices.Add((short)(i2 + 1));
                    indices.Add(i2); indices.Add((short)(i2 + 1)); indices.Add((short)(i2 - sideCount.Value + 1));
				}
            }

			//End Cap
            vertices.Add(new VertexPositionColor(new Vector3(0, 0, zSize.Value), Color.GreenYellow));
            for (int s = 0; s < sideCount.Value - 1; s++)
            {
                indices.Add((short)(vertices.Count - 1)); indices.Add((short)(vertices.Count - sideCount.Value + s)); indices.Add((short)(vertices.Count - sideCount.Value + s - 1));
            }
            indices.Add((short)(vertices.Count - 1)); indices.Add((short)(vertices.Count - sideCount.Value - 1)); indices.Add((short)(vertices.Count - 2));

            VertexBuffer = new VertexBuffer(_graphics.GraphicsDevice, typeof(VertexPositionColor), vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices.ToArray());
            IndexBuffer = new IndexBuffer(_graphics.GraphicsDevice, typeof(short), indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices.ToArray());
        }
	}
}