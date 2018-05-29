using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace ServerSide
{
    class Program
    {
        private static readonly ConcurrentDictionary<uint, object> PrimeNumbers = new ConcurrentDictionary<uint, object>();
        static void Main()
        {
            PrimeNumbers[2] = null;
            PrimeNumbers[3] = null;

            using (var server = new ResponseSocket("@tcp://localhost:5556"))
            {
                string msg;
                do
                {
                    msg = server.ReceiveFrameString();
                    Console.WriteLine($"From Client: {msg}");

                    if (!uint.TryParse(msg, out uint number))
                    {
                        server.SendFrame($"{msg} is not a number!");
                        continue;
                    }

                    if (number == 1)
                    {
                        server.SendFrame("One is not a prime.");
                        continue;
                    }

                    var primeFactors = PrimeFactorsOf(number);
                    // Send a response back from the server
                    server.SendFrame(primeFactors);
                } while (msg != "done");
            }
        }

        private static string PrimeFactorsOf(uint number)
        {
            var n = number;
            List<uint> primeFactors = new List<uint>();

            foreach (var primeNumber in PrimeNumbers.Keys)
            {
                while (n % primeNumber == 0)
                {
                    n /= primeNumber;
                    primeFactors.Add(primeNumber);
                }

                if (n == 1)
                    return ListToMessage(number, primeFactors);
            }

            for (uint i = 5; i < n; i += 2)
            {
                while (n % i == 0)
                {
                    n /= i;
                    primeFactors.Add(i);
                    PrimeNumbers[i] = null;
                }              
            }
            if (n != 1)
            {
                primeFactors.Add(n);
                PrimeNumbers[n] = null;
            }
            return ListToMessage(number, primeFactors);
        }

        private static string ListToMessage(uint number, List<uint> primeFactors)
        {
            if (primeFactors.Count > 10)
                Debugger.Break();
            var list = primeFactors.Aggregate(new StringBuilder(), (sb, i) => sb.Append($"{i},"), sb=>sb.Remove(sb.Length - 1, 1).ToString());
            var result = $"{number}: {list}";
            return result;
        }
    }
}
