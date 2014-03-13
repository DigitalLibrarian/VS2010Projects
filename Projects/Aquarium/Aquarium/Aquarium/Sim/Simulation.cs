using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;
using Forever.Render;
using Aquarium.Sim.Agents;

namespace Aquarium.Sim
{
    public class Simulation
    {
        public SimSpace Space { get; private set; }
        public PartitionSphere<IAgent> UpdateSet { get; set; }
        public PartitionSphere<IAgent> DrawSet { get; set; }

        float UpdateRadius = 2500;
        float DrawRadius = 5000;

        public Simulation()
        {
            Space = new SimSpace(gridSize: 500, foodSpaceGridSize: 100);
            UpdateSet = new PartitionSphere<IAgent>(Space, GetUpdateSphere());
            DrawSet = new PartitionSphere<IAgent>(Space, GetDrawSphere());
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
