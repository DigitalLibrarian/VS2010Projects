using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Neural;
using Microsoft.Xna.Framework;

namespace Pong.ArenaMatch
{
    public class NeuralPaddle : Paddle
    {

        NeuralNetwork Network { get; set; }
        List<List<double>> LastRun { get; set; }


        Paddle TeacherPaddle { get; set; }
        public bool Train { get; set; }
        public double Error { get; set; }

        public NeuralPaddle(Paddle teacherPaddle, Vector2 position, float length, float width, Random random = null, float speed = 0.1f)
            : base(position, length, width, speed)
        {
            if (random != null)
            {
                GenerateRandomNetwork(random);
            }
            LastRun = new List<List<double>>();
            TeacherPaddle = teacherPaddle;
            Color = Color.Red;
            Train = false;
        }

        private void GenerateRandomNetwork(Random random)
        {
            int numInputs = 1;
            int numHiddenLayer = 1;
            int numOutputs = 1;

            Network = new NeuralNetwork(numInputs, numHiddenLayer, numOutputs);


            Network.RandomizeWeights(random);

        }

        Random R = new Random();
        public override Vector2 ComputeChange(Ball ball, Paddle paddle)
        {

            var ballRise = ball.RiseDirection;
            var ballPos = ball.Position;
            var ballRad = ball.Radius;

            var inputs = new List<double>();

            inputs.Add((int)ballPos.Y - (int)Position.Y);
            
            var outputs = Network.ComputeOutputs(inputs.ToArray());


            double yChange = outputs.First(); 
            if (Train)
            {

                var answerVect = TeacherPaddle.ComputeChange(ball, this);
                double answer =  answerVect.Y > 0  ? 1 : - 1;
                var trainIn = inputs ;
                var trainOut = new List<double>() { answer };
                Error = Network.Train(trainIn, trainOut, learnRate: 0.001, maxEpochs: 100, errorThresh: 0.00, momentum: 0.01);
            }

            if (yChange > 0) yChange = 1;
            if (yChange < 0) yChange = -1;
            return new Vector2(0f, (float) yChange);

        }


        public override void Update(Arena arena, Ball ball, float duration)
        {
            LastRun.Clear();

            TeacherPaddle.Update(arena, ball, duration);

            base.Update(arena, ball, duration);
        }
    }
}
