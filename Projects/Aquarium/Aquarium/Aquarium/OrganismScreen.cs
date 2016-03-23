using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Render;
using Forever.Physics;
using Aquarium.Life;
using Aquarium.Ui;
using Microsoft.Xna.Framework.Graphics;
using Aquarium.Life.Genomes;
using Forever.Render.Cameras;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Aquarium.Life.Spec;
using Forever.Extensions;

namespace Aquarium
{
    class OrganismScreen : UiOverlayGameScreen
    {
        Organism Organism { get; set; }
        Organism NextOrganism { get; set; }
        object _noLock = new object();

        OrganismSpecParser SpecParser { get; set; }

        public void LoadOrganism(Organism organism)
        {
            Organism = organism;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            RenderContext.Camera.Position = Vector3.Backward * 200;
            
            OrgWorld = Matrix.Identity;

            SpecParser = new OrganismSpecParser();
            SpecParser.MaxBodyParts = 100;
            SpecParser.MinBodyParts = 75;
        }

        Organism RandomOrganism()
        {
            var r = new Random();
            Organism o = null;
            while (o == null)
            {
                var genome = new BodyGenome(r.NextIntegers(100));

                o = Organism.CreateFromGenome(genome, SpecParser);

                if (o != null)
                {
                    o.RigidBody.calculateDerivedData();
                    o.Body.World = OrgWorld;
                }
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
