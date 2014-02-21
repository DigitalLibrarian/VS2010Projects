using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;

namespace Forever.Physics
{
    public static class InertiaTensorFactory
    {

        public static Matrix Sphere(float mass, float radius)
        {
            float sphere_coeff = ((2f/3f) * mass * radius * radius);
            Matrix inertiaTensor = new Matrix(
                sphere_coeff, 0f, 0f, 0f,
                0f, sphere_coeff, 0f, 0f,
                0f, 0f, sphere_coeff, 0f,
                0f, 0f, 0f, 1f
            );

            return inertiaTensor;
        }


        public static Matrix Cubiod(float height, float width, float depth, float mass)
        {
            float cuboidCoeff = mass / 12f;
            Matrix inertiaTensor = new Matrix(
                cuboidCoeff * (width * width * depth * depth ), 0f, 0f, 0f,
                0f, cuboidCoeff * ( height * height * depth * depth), 0f, 0f,
                0f, 0f, cuboidCoeff * (height * height * width   *  width ), 0f,
                0f, 0f, 0f, 1f
            );

            return inertiaTensor;

        }
    }
}
