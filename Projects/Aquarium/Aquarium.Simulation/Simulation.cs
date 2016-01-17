using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Render;
using Forever.SpacePartitions;
using Forever.Physics;

namespace Aquarium.Sim
{
    public class Simulation
    {
        public SimSpace Space { get; private set; }
        public PartitionSphere<ISimObject> UpdateSet { get; set; }
        public PartitionSphere<ISimObject> DrawSet { get; set; }

        float UpdateRadius = 2500;
        float DrawRadius = 5000;

        public Simulation()
        {
            Space = new SimSpace(gridSize: 500, foodSpaceGridSize: 500);
            UpdateSet = new PartitionSphere<ISimObject>(Space, GetUpdateSphere());
            DrawSet = new PartitionSphere<ISimObject>(Space, GetDrawSphere());
        }

        const int RefetchFrequency = 10000;
        int timeLeft = 0;
        public void Update(GameTime gameTime)
        {

            timeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            if (timeLeft <= 0)
            {
                DrawSet.Refetch();
                UpdateSet.Refetch();
                timeLeft = RefetchFrequency;
            }
            else
            {
                UpdateSet.Sphere = GetUpdateSphere();
            }

            var duration = gameTime.GetDuration();
            foreach (var agent in UpdateSet)
            {
                agent.Update(duration);
            }
        }

        public void Draw(GameTime gameTime, RenderContext renderContext)
        {
            DrawSet.Sphere = GetDrawSphere();

            var duration = gameTime.GetDuration();
            foreach (var agent in DrawSet)
            {
                agent.Draw(duration, renderContext);
            }
        }

        private BoundingSphere GetUpdateSphere()
        {
            return new BoundingSphere(Vector3.Zero, UpdateRadius);
        }

        private BoundingSphere GetDrawSphere()
        {
            return new BoundingSphere(Vector3.Zero, DrawRadius);
        }
    }


}
