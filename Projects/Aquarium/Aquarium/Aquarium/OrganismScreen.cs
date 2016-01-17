using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Render;

using Aquarium.Life;
using Aquarium.Ui;
using Microsoft.Xna.Framework.Graphics;
using Aquarium.Life.Genomes;
using Forever.Render.Cameras;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace Aquarium
{
    class OrganismScreen : UiOverlayGameScreen
    {
        int DefaultParts = 20;

        int DefaultOrgans = 20;

        int DefaultNN = 1;

        int DefaultJunk = 0;

        Organism Organism { get; set; }
        Organism NextOrganism { get; set; }
        object _noLock = new object();

        public void LoadOrganism(Organism organism)
        {
            Organism = organism;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var gd = ScreenManager.GraphicsDevice;
            var cam = new EyeCamera(gd);
            cam.Position = Vector3.Backward * 20;
            RenderContext = new RenderContext(
                    cam,
                    gd
                );
            Ui = new UiOverlay(
                ScreenManager,
                RenderContext
               );

            OrgWorld = Matrix.Identity;
        }

        Organism RandomOrganism()
        {
            var r = new Random();
            Organism o = null;
            while (o == null)
            {
                var genome = BodyGenome.Random(
                    r,
                    numParts: DefaultParts,
                    numOrgans: DefaultOrgans,
                    numNN: DefaultNN,
                    sizeJunk: DefaultJunk
                    );

                o = Organism.CreateFromGenome(genome);
                o.RigidBody.calculateDerivedData();
                o.Body.World = OrgWorld;
            }
            return o;
        }

        public override void Draw(GameTime gameTime)
        {
            if (Organism != null) Organism.Body.Render(RenderContext);

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!_restartCycle)
            {
                if (NextOrganism != null)
                {
                    lock (_noLock)
                    {
                        Organism = NextOrganism;
                        NextOrganism = null;
                    }
                }
                StartWaiting();
            }


            if (Organism != null)
            {
                OrgWorld *= Matrix.CreateFromYawPitchRoll(0.01f, 0.00f, 0.01f);
                Organism.Body.World = OrgWorld;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        public Matrix OrgWorld { get; set; }

        void StartBackgroundSpawn()
        {
            var task = new Task(() =>
            {
                lock (_noLock)
                {
                    NextOrganism = null;
                }
                var org = RandomOrganism();
                lock (_noLock)
                {
                    NextOrganism = org;
                }

                _restartCycle = false;
            });

            task.Start();
        }

        bool _restartCycle = false;

        private void StartWaiting()
        {
            var task = new Task(() =>
            {
                System.Threading.Thread.Sleep(3000);
                StartBackgroundSpawn();
            });

            _restartCycle = true;
            task.Start();
        }
    }
}
