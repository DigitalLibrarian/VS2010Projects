using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Screens;
using Aquarium.GA.Population;
using System.Threading;
using Forever.Render;
using Microsoft.Xna.Framework;

namespace Aquarium
{
    public class SimScreen : GameScreen
    {
        Thread GenerateThread;

        public Population Pop { get; private set; }

        protected RenderContext RenderContext { get; private set; }

        public SimScreen(RenderContext renderContext)
        {
            RenderContext = renderContext;
            Pop = new RandomPopulation(100, 1000);

            GenerateThread = new Thread(new ThreadStart(() => { }));
        }

        public override void LoadContent()
        {
            base.LoadContent();

            GenerateThread.IsBackground = true;
            GenerateThread.Start();
        }
        public override void UnloadContent()
        {

            GenerateThread.Abort();
            System.Threading.SpinWait.SpinUntil(() => 
                {
                    System.Threading.Thread.Sleep(100);
                    return !GenerateThread.IsAlive;
                }
                );

            base.UnloadContent();


        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            var members = Pop.LocalMembers(RenderContext.Camera.Position);

            foreach (var member in members)
            {
                member.Specimen.Body.Render(RenderContext);
            }

            base.Draw(gameTime);
        }

        private void CamTrackMember(PopulationMember mem)
        {
            var body = mem.Specimen.Body;
            var Camera = RenderContext.Camera;
            var min = new Vector3();
            var max = new Vector3();
            foreach (var part in body.Parts)
            {
                var bsc = part.BodySpaceCorners();

                foreach (var vec in bsc)
                {
                    min = Vector3.Min(vec, min);
                    max = Vector3.Max(vec, max);
                }
            }
            var s = BoundingSphere.CreateFromBoundingBox(new BoundingBox(min, max));
            Camera.Position = Vector3.UnitZ * s.Radius * 3f;

        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
           // if (!otherScreenHasFocus)
            {
                var members = Pop.LocalMembers(RenderContext.Camera.Position);
                foreach (var member in members)
                {
                    member.Specimen.Body.Update((float)gameTime.ElapsedGameTime.Milliseconds);
                }

                var watched = members.OrderBy(x => x.Score).First();

                CamTrackMember(watched);
            }



            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
