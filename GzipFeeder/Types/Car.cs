namespace GzipFeeder.Types
{
    public class Car
    {
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        
        public static List<Car> GenerateTestCars(int count = 1000)
        {
            var cars = new List<Car>();
            for (int i = 0; i < count; i++)
            {
                cars.Add(new Car
                {
                    Model = $"Model {i}",
                    Year = 2024- i,
                    Price = 10000 + i
                });
            }
            return cars;
        }   
    }
}
