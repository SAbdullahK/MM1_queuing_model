using System;

namespace QueuingModels
{
    /// <summary>
    /// Analytical (closed-form) M/M/1 queuing model.
    /// Takes mean arrival rate (lambda) and mean service rate (mu) as input —
    /// no simulation, no random number generation. Pure steady-state formulas.
    /// </summary>
    public class MM1QueueModel
    {
        public double Lambda { get; }   // mean arrival rate  (customers per unit time)
        public double Mu { get; }       // mean service rate  (customers per unit time)
        public double Rho { get; }      // utilization factor = lambda / mu

        public MM1QueueModel(double lambda, double mu)
        {
            if (lambda <= 0) throw new ArgumentException("Arrival rate (lambda) must be > 0.");
            if (mu <= 0) throw new ArgumentException("Service rate (mu) must be > 0.");

            Rho = lambda / mu;
            if (Rho >= 1)
                throw new ArgumentException(
                    $"System is unstable (rho = {Rho:F3} >= 1). Need lambda < mu.");

            Lambda = lambda;
            Mu = mu;
        }

        // Convenience constructor: build from mean interarrival time and mean service time
        public static MM1QueueModel FromMeanTimes(double meanInterarrivalTime, double meanServiceTime)
            => new MM1QueueModel(1.0 / meanInterarrivalTime, 1.0 / meanServiceTime);

        // --- Steady-state performance measures ---

        /// P0: probability system is empty
        public double P0 => 1 - Rho;

        /// Probability of exactly n customers in the system
        public double Pn(int n)
        {
            if (n < 0) throw new ArgumentException("n must be >= 0.");
            return (1 - Rho) * Math.Pow(Rho, n);
        }

        /// Probability of n or more customers in the system
        public double PAtLeast(int n) => Math.Pow(Rho, n);

        /// L: expected number of customers in the system
        public double L => Rho / (1 - Rho);

        /// Lq: expected number of customers in the queue (waiting, not being served)
        public double Lq => Rho * Rho / (1 - Rho);

        /// Ls: expected number of customers in service (0 or 1 for M/M/1) — same as Rho
        public double Ls => Rho;

        /// W: expected time a customer spends in the system (wait + service)
        public double W => 1.0 / (Mu - Lambda);

        /// Wq: expected time a customer spends waiting in queue
        public double Wq => Rho / (Mu - Lambda);

        /// Probability that an arriving customer has to wait (system busy)
        public double ProbWait => Rho;

        public void PrintSummary(int maxN = 5)
        {
            Console.WriteLine("===== M/M/1 Queuing Model — Steady State Results =====");
            Console.WriteLine($"Lambda (arrival rate)   : {Lambda:F4} per unit time");
            Console.WriteLine($"Mu     (service rate)   : {Mu:F4} per unit time");
            Console.WriteLine($"Rho    (utilization)    : {Rho:F4}");
            Console.WriteLine();
            Console.WriteLine($"P0  (P system is empty) : {P0:F4}");
            Console.WriteLine($"L   (avg # in system)   : {L:F4}");
            Console.WriteLine($"Lq  (avg # in queue)    : {Lq:F4}");
            Console.WriteLine($"Ls  (avg # in service)  : {Ls:F4}");
            Console.WriteLine($"W   (avg time in system): {W:F4}");
            Console.WriteLine($"Wq  (avg time in queue) : {Wq:F4}");
            Console.WriteLine($"P(wait > 0)             : {ProbWait:F4}");
            Console.WriteLine();
            Console.WriteLine("Pn  (probability of exactly n in system):");
            for (int n = 0; n <= maxN; n++)
                Console.WriteLine($"  P({n}) = {Pn(n):F4}   P(n>={n}) = {PAtLeast(n):F4}");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("M/M/1 Analytical Queuing Model");
            Console.WriteLine("Choose input type:");
            Console.WriteLine("  1 = mean arrival rate (lambda) & mean service rate (mu)");
            Console.WriteLine("  2 = mean interarrival time & mean service time");
            Console.Write("Enter 1 or 2: ");
            string choice = Console.ReadLine()?.Trim();

            try
            {
                MM1QueueModel model;

                if (choice == "2")
                {
                    Console.Write("Mean interarrival time (e.g. minutes/customer): ");
                    double ta = double.Parse(Console.ReadLine());
                    Console.Write("Mean service time (e.g. minutes/customer): ");
                    double ts = double.Parse(Console.ReadLine());
                    model = MM1QueueModel.FromMeanTimes(ta, ts);
                }
                else
                {
                    Console.Write("Mean arrival rate lambda (customers per unit time): ");
                    double lambda = double.Parse(Console.ReadLine());
                    Console.Write("Mean service rate mu (customers per unit time): ");
                    double mu = double.Parse(Console.ReadLine());
                    model = new MM1QueueModel(lambda, mu);
                }

                Console.WriteLine();
                model.PrintSummary(maxN: 5);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}