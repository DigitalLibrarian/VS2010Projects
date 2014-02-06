using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Render;
using Aquarium.GA.Organs;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Bodies
{
    public class BodyPart
    {
        public Vector3 LocalPosition { get; set; }
        /// <summary>
        ///  Matrix that transforms unit cube into body part shape
        /// </summary>
        public Matrix UCTransform { get; set; }
        public Matrix Rotation { get; set; }
        public Matrix BodyWorld { get { return UCTransform * Rotation; } }
        public List<BodyPartSocket> Sockets { get; set; }
        public List<Organ> Organs { get; set; }
        public Color Color { get; set; }

        public ChanneledSignal ChanneledSignal { get; set; }
        
        public BodyPart()
        {
            Sockets = new List<BodyPartSocket>();
            Organs = new List<Organ>();
            UCTransform = Matrix.Identity;
            Rotation = Matrix.Identity;

            ChanneledSignal = new ChanneledSignal(new List<double>());
        }

        public void AddOrgan(Organ organ)
        {
            Organs.Add(organ);
        }

        IEnumerable<Organ> FindReachable(BodyPart bodypart, Predicate<Organ> tester)
        {
            var list = new List<Organ>();
            foreach (var sock in Sockets)
            {
                if (sock.HasAvailable) continue;

                var fPart = sock.ForeignSocket.Part;

                foreach (var organ in fPart.Organs)
                {
                    if (tester(organ)) list.Add(organ);
                }

            }
            return list;
        }

        public void Update(Body body, float duration)
        {

        }


        public virtual void Render(Body body, RenderContext renderContext)
        {
            var color = Color;
            var world = Matrix.CreateTranslation(body.Position + LocalPosition) * body.World;
            Renderer.RenderUnitCubeTransform(renderContext, BodyWorld, world, color, wireFrame: false);

            foreach (var socket in Sockets)
            {
                world = Matrix.CreateTranslation(body.Position + LocalPosition + socket.LocalPosition) * body.World;
                var ucTransform = Matrix.CreateScale(0.05f) * UCTransform;
                Renderer.RenderUnitCubeTransform(renderContext, ucTransform, world, Color.Red, wireFrame: true);
            }
        }


        public BoundingBox UnitCubeBB()
        {
            return new BoundingBox(new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));
        }

        public Vector3[] BodySpaceCorners()
        {
            var box = UnitCubeBB();
            var corners = box.GetCorners();

            for (int i = 0; i < corners.Count(); i++)
            {
                corners[i] = LocalPosition + Vector3.Transform(corners[i], BodyWorld);
            }
            return corners;
        }

        public BoundingBox BodyBB()
        {
            var corners = BodySpaceCorners();
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < corners.Count(); i++)
            {
                min = Vector3.Min(corners[i], min);
                max = Vector3.Max(corners[i], max);
            }

            return new BoundingBox(min, max);
        }

    }

   

    public class TestBodyPart : BodyPart
    {
    }
}
