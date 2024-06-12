using GzipFeeder.FeederService;
using GzipFeeder.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GzipFeeder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", false)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IFeed, EvenHubFeeder>()
                .BuildServiceProvider();

            var feeder = serviceProvider.GetService<IFeed>();
            var t1 = feeder.FeedAsync(Car.GenerateTestCars(), nameof(Car), "Car_evh_json_mapping");
            var t2= feeder.FeedAsync(User.GenerateTestUsers(), nameof(User), "User_evh_json_mapping");

            await Task.WhenAll(t1, t2);
            Console.WriteLine("Done");
        }
    }
}
