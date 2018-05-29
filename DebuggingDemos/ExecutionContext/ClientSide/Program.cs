﻿using System;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace ClientSide
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting client side.");
            SendMessages();
        }

        private static void SendMessages()
        {
            object zeroMqLock = new object();

            using (var client = new RequestSocket(">tcp://localhost:5556")) // connect
            {
                Parallel.For(1, 20000, i =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    SendAMessage(zeroMqLock, client, i);
                    Task.Delay(100);
                });
                client.SendFrame($"done");
            }
        }

        private static void SendAMessage(object zeroMqLock, RequestSocket client, int i)
        {
            string msg;
            lock (zeroMqLock)
            {
                client.SendFrame($"{i}");
                msg = client.ReceiveFrameString();
            }

            Console.WriteLine($"From Server: {msg}");
        }
    }
}
