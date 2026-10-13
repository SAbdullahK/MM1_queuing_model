using System;

namespace QueuingModels
{
    /// <summary>
    /// Analytical M/M/c queuing model: c identical servers sharing one queue.
    /// Formulas follow the course notes:
    ///   c = 1  -> the M/M/1 set (Lq, Wq, W, L = lambda * W, idle time 1 - rho)
    ///   c > 1  -> the M/M/c set (rho0, Lq, Wq, L = Lq + a, W = L / lambda)
    /// Pure steady-state formulas, no simulation.
    /// </summary>
    public class MMcQueueModel
    {
        public double Lambda { get; }        // mean arrival rate
        public double Mu { get; }            // mean service rate of ONE server
        public int Servers { get; }          // c, number of identical servers
        public double OfferedLoad { get; }   // a = lambda / mu
        public double Rho { get; }           // rho = lambda / (c * mu), must be < 1
        public double IdleFraction { get; }  // 1 - rho, fraction of time a server is idle
        public double Rho0 { get; }          // rho0, probability the system is empty
        public double Lq { get; }            // expected number waiting in queue
        public double Wq { get; }            // expected time waiting in queue
        public double W { get; }             // expected time in system
        public double L { get; }             // expected number in system

        public MMcQueueModel(double lambda, double mu, int servers)
        {
            if (lambda <= 0) throw new ArgumentException("Arrival rate (lambda) must be > 0.");
            if (mu <= 0) throw new ArgumentException("Service rate (mu) must be > 0.");
            if (servers < 1) throw new ArgumentException("Number of servers must be at least 1.");

            Lambda = lambda;
            Mu = mu;
            Servers = servers;

            // a = lambda / mu
            OfferedLoad = lambda / mu;

            // rho = lambda / (c * mu)
            Rho = lambda / (servers * mu);
            if (Rho >= 1)
                throw new ArgumentException(
                    $"System is unstable (rho = {Rho:F3} >= 1). Need lambda < c * mu.");

            // Server idle time = 1 - rho
            IdleFraction = 1 - Rho;

            double a = OfferedLoad;
            double c = servers;

            // rho0 = [ sum_{n=0}^{c-1} a^n / n!  +  (a^c / c!) * (1 / (1 - rho)) ]^-1
            double sum = 0;
            for (int n = 0; n <= servers - 1; n++)
                sum += Math.Pow(a, n) / Factorial(n);
            Rho0 = 1.0 / (sum + (Math.Pow(a, servers) / Factorial(servers)) * (1.0 / (1 - Rho)));

            if (servers == 1)
            {
                // ---- M/M/1 formulas ----
                Lq = (Rho * Rho) / (1 - Rho);   // Lq = rho^2 / (1 - rho)
                Wq = Lq / lambda;               // Wq = Lq / lambda
                W = Wq + 1.0 / mu;              // W  = Wq + 1/mu
                L = lambda * W;                 // L  = lambda * W
            }
            else
            {
                // ---- M/M/c formulas ----
                // Lq = rho0 * a^c * rho / ( c! * (1 - rho)^2 )
                Lq = Rho0 * Math.Pow(a, servers) * Rho / (Factorial(servers) * Math.Pow(1 - Rho, 2));
                Wq = Lq / lambda;               // Wq = Lq / lambda
                L = Lq + a;                     // L  = Lq + lambda / mu
                W = L / lambda;                 // W  = L / lambda  (= Wq + 1/mu)
            }
        }

        // Convenience constructor: build from mean interarrival time and mean service time (per server)
        public static MMcQueueModel FromMeanTimes(double meanInterarrivalTime, double meanServiceTime, int servers)
            => new MMcQueueModel(1.0 / meanInterarrivalTime, 1.0 / meanServiceTime, servers);

        private static double Factorial(int n)
        {
            double f = 1;
            for (int i = 2; i <= n; i++) f *= i;
            return f;
        }
    }
}