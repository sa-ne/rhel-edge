using Prometheus;
using System;
using System.Threading;


namespace prometheus_dotnetcore_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello and welcome to the prometheus dotnetcore demo app!");
            Console.WriteLine("We will attempt to mimic a live system's behaviour in terms of warnings and exceptions." + Environment.NewLine);
            int milliSecondsToSleep = 500;

            SetupRequirements();

            //interval settings
            int loop_counter = 0;
            const int warning_interval = 20;
            const int exception_interval = 50;

            //scrape counters
            var prom_ok = Metrics.CreateCounter("prom_ok", "This fields indicates the transactions that were processed correctly.");
            var prom_warning = Metrics.CreateCounter("prom_warning", "This fields indicates the warning count.");
            var prom_exception = Metrics.CreateCounter("prom_exception", "This fields indicates the exception count.");

            while (true)
            {
                //main control loop
                Console.WriteLine("Transaction processed: OK"); prom_ok.Inc(1);
                Thread.Sleep(milliSecondsToSleep);
                if(loop_counter == warning_interval)
                {
                    prom_warning.Inc(1);
                    Console.WriteLine("\nOops that was a warning  - treat carefully...\n");
                }
                else if (loop_counter == exception_interval)
                {
                    prom_exception.Inc(1);
                    Console.WriteLine("\nAlarm! call 911 - an exception has occured!\n");
                    loop_counter = 0;
                }
                loop_counter++;
            }
        }

        private static void SetupRequirements()
        {
            var metricServer = new MetricServer(port: 8080);
            metricServer.Start();
        }

        public void ConfigureServices(IServiceCollection services)  
        {  
            services.AddHealthChecksUI();  
        }  
  
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)  
        {  
            app.UseHealthChecksUI();  
        } 
    }
}
