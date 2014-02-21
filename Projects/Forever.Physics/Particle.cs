using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics
{
    public class Particle 
    {
        public Particle(Vector3 position)
        {
            _Position = position;
            _Velocity = Vector3.Zero;
            _Acceleration = Vector3.Zero;
        }

        Vector3 _Position, _Velocity, _Acceleration;
        public Vector3 Position { get { return _Position; } }
        public Vector3 Velocity { get { return _Velocity; } }
        public Vector3 Acceleration { get { return _Acceleration; } }

        private Vector3 forceAccum;
        private void clearAccumulator()
        {
            forceAccum = Vector3.Zero;
        }
        public void addForce(Vector3 force)
        {
            forceAccum += force;
        }
        /// <summary>
        /// Holds the amount of damping applied to linear motion.
        /// Damping is required to remove energy added through
        /// round-off error and stuff.  This is used to control velocity.
        /// 
        /// This breaks the First law
        /// </summary>
        float linear = 1.0f;
        public float LinearDamping { get { return linear; } set { linear = value; } }
        float _InverseMass;
        /// <summary>
        /// Storing the mass this way helps with the math and allows 
        /// us to create infinetly massed objects.
        /// 
        /// Set to zero for infinetly massive objects
        /// </summary>
        public float InverseMass { get { return _InverseMass; } set { _InverseMass = value; } }

        /// <summary>
        /// Mass is the measure of inertia of an object.
        /// </summary>
        public float Mass { get { return _InverseMass / 1; } set { _InverseMass = (value / 1); } }


        public  void integrate(float duration)
        {
            if (duration < 0.0)
            {
                throw new InvalidOperationException("Particle class was told to integrate with a negative time duration : You can't go back in time!");
            }

            
            _Position += _Velocity * duration;
            Vector3 result = _Acceleration;
            result += forceAccum * _InverseMass;
            _Velocity += result * duration;
            _Velocity *= (float)Math.Pow(linear, duration);
            clearAccumulator();
        }


        public void setVelocity(Vector3 vec)
        {
            _Velocity = vec;
        }


        public void setPosition(Vector3 vec)
        {
            _Position = vec;
        }

    }
}
