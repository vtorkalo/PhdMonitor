
using guider;
using System;
using System.Threading;

namespace PHD2Client
{
    class Program
    {
        static void WaitForSettleDone(Guider guider)
        {
            while (true)
            {
                SettleProgress s = guider.CheckSettling();

                if (s.Done)
                {
                    Console.WriteLine("settling is done");
                    break;
                }

                Console.WriteLine("settling dist {0:F1}/{1:F1}  time {2:F1}/{3:F1}",
                       s.Distance, s.SettlePx, s.Time, s.SettleTime);

                Thread.Sleep(1000);
            }
        }

        static void Main(string[] args)
        {
            string host = "localhost";
            if (args.Length > 0)
                host = args[0];
            try
            {
                using (Guider guider = Guider.Factory(host))
                {
                    guider.Connect();
                    var s = guider.GetStats();

                    double settlePixels = 2.0;
                    double settleTime = 20.0;
                    double settleTimeout = 60.0;

                    for (;;)
                    {
                        string state;
                        double avgDist;
                        guider.GetStatus(out state, out avgDist);

                        Console.WriteLine("{0} dist={1:F1}",
                               state, avgDist);

                        Thread.Sleep(2000);
                        if (state == "LostLock" || state == "Looping")
                        {
                            guider.StopCapture();
                            Thread.Sleep(5000);
                            guider.Guide(settlePixels, settleTime, settleTimeout);
                            WaitForSettleDone(guider);
                            Thread.Sleep(5000);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("Error: {0}", err.Message);
            }

            Console.ReadLine();
        }
    }
}
