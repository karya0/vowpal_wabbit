﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiWorldTesting;

namespace cs_test
{
    class ExploreSample
    {
        private static UInt32 MyStatelessPolicyFunc(IntPtr applicationContext)
        {
            return 222;
        }

        private static UInt32 MyStatefulPolicyFunc(IntPtr policyParams, IntPtr applicationContext)
        {
            return 111;
        }

        private static void MyStatefulScorerFunc(IntPtr policyParams, IntPtr applicationContext, IntPtr scoresPtr, uint size)
        {
            float[] scores = MwtExplorer.IntPtrToScoreArray(scoresPtr, size);
            for (uint i = 0; i < size; i++)
            {
                scores[i] = (int)policyParams + i;
            }
        }
        private static void MyStatelessScorerFunc(IntPtr applicationContext, IntPtr scoresPtr, uint size)
        {
            float[] scores = MwtExplorer.IntPtrToScoreArray(scoresPtr, size);
            for (uint i = 0; i < size; i++)
            {
                scores[i] = i;
            }
        }

        public static void Run()
        {
            MwtExplorer mwt = new MwtExplorer();

            uint numActions = 10;
            
            float epsilon = 0.2f;
            uint tau = 5;
            uint bags = 10;
            float lambda = 0.5f;

            int policyParams = 1003;

            /*** Initialize Epsilon-Greedy explore algorithm using a default policy function that accepts parameters ***/
            mwt.InitializeEpsilonGreedy(epsilon, new StatefulPolicyDelegate(MyStatefulPolicyFunc), new IntPtr(policyParams), numActions);

            /*** Initialize Epsilon-Greedy explore algorithm using a stateless default policy function ***/
            //mwt.InitializeEpsilonGreedy(epsilon, new StatelessPolicyDelegate(MyStatelessPolicyFunc), numActions);

            /*** Initialize Tau-First explore algorithm using a default policy function that accepts parameters ***/
            //mwt.InitializeTauFirst(tau, new StatefulPolicyDelegate(MyStatefulPolicyFunc), new IntPtr(policyParams), numActions);

            /*** Initialize Tau-First explore algorithm using a stateless default policy function ***/
            //mwt.InitializeTauFirst(tau, new StatelessPolicyDelegate(MyStatelessPolicyFunc), numActions);

            /*** Initialize Bagging explore algorithm using a default policy function that accepts parameters ***/
            //StatefulPolicyDelegate[] funcs = 
            //{
            //    new StatefulPolicyDelegate(MyStatefulPolicyFunc), 
            //    new StatefulPolicyDelegate(MyStatefulPolicyFunc) 
            //};
            //IntPtr[] parameters = { new IntPtr(policyParams), new IntPtr(policyParams) };
            //mwt.InitializeBagging(bags, funcs, parameters, numActions);

            /*** Initialize Bagging explore algorithm using a stateless default policy function ***/
            //StatelessPolicyDelegate[] funcs = 
            //{
            //    new StatelessPolicyDelegate(MyStatelessPolicyFunc), 
            //    new StatelessPolicyDelegate(MyStatelessPolicyFunc) 
            //};
            //mwt.InitializeBagging(bags, funcs, numActions);

            /*** Initialize Softmax explore algorithm using a default policy function that accepts parameters ***/
            //mwt.InitializeSoftmax(lambda, new StatefulScorerDelegate(MyStatefulScorerFunc), new IntPtr(policyParams), numActions);

            /*** Initialize Softmax explore algorithm using a stateless default policy function ***/
            //mwt.InitializeSoftmax(lambda, new StatelessScorerDelegate(MyStatelessScorerFunc), numActions);

            FEATURE[] f = new FEATURE[2];
            f[0].X = 0.5f;
            f[0].WeightIndex = 1;
            f[1].X = 0.9f;
            f[1].WeightIndex = 2;

            string otherContext = "Some other context data that might be helpful to log";
            CONTEXT context = new CONTEXT(f, otherContext);

            UInt32 chosenAction = mwt.ChooseAction(context, "myId");

            string interactions = mwt.GetAllInteractionsAsString();

            Console.WriteLine(chosenAction);
            Console.WriteLine(interactions);
        }

        public static void Clock()
        {
            float epsilon = .2f;
            int policyParams = 1003;
            string uniqueKey = "clock";
            int numFeatures = 1000;
            int numIter = 1000;
            int numWarmup = 1000;
            int numInteractions = 1;
            uint numActions = 10;
            string otherContext = null;
            
            double timeInit = 0, timeChoose = 0, timeSerializedLog = 0, timeTypedLog = 0;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            for (int iter = 0; iter < numIter + numWarmup; iter++)
            {
                watch.Restart();
                
                MwtExplorer mwt = new MwtExplorer();
                mwt.InitializeEpsilonGreedy(epsilon, new StatefulPolicyDelegate(MyStatefulPolicyFunc), new IntPtr(policyParams), numActions);

                timeInit += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;

                FEATURE[] f = new FEATURE[numFeatures];
                for (int i = 0; i < numFeatures; i++)
                {
                    f[i].WeightIndex = (uint)i + 1;
                    f[i].X = 0.5f;
                }
                
                watch.Restart();

                CONTEXT context = new CONTEXT(f, otherContext);

                for (int i = 0; i < numInteractions / 2; i++)
                {
                    mwt.ChooseAction(context, uniqueKey);
                    mwt.ChooseActionAndKey(context);
                }

                timeChoose += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;

                watch.Restart();

                string interactions = mwt.GetAllInteractionsAsString();

                timeSerializedLog += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;

                for (int i = 0; i < numInteractions / 2; i++)
                {
                    mwt.ChooseAction(context, uniqueKey);
                    mwt.ChooseActionAndKey(context);
                }

                watch.Restart();

                mwt.GetAllInteractions();

                timeTypedLog += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;
            }
            Console.WriteLine("--- PER ITERATION ---");
            Console.WriteLine("# iterations: {0}, # interactions: {1}, # context features {2}", numIter, numInteractions, numFeatures);
            Console.WriteLine("Init: {0} micro", timeInit * 1000 / numIter);
            Console.WriteLine("Choose Action: {0} micro", timeChoose * 1000 / (numIter * numInteractions));
            Console.WriteLine("Get Serialized Log: {0} micro", timeSerializedLog * 1000 / numIter);
            Console.WriteLine("Get Typed Log: {0} micro", timeTypedLog * 1000 / numIter);
            Console.WriteLine("--- TOTAL TIME: {0} micro", (timeInit + timeChoose + timeSerializedLog + timeTypedLog) * 1000);
        }
    }
}