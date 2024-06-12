namespace GzipFeeder.Types
{
    public class User
    {
        public string Name { get; set; }
        public string Surname { get; set; } 
        public int Age { get; set; }
        public bool IsHuman { get; set; }

         
        public static List<User> GenerateTestUsers(int count = 1000)
        {
            var users = new List<User>();
            for (int i = 0; i < count; i++)
            {
                users.Add(new User
                {
                    Name = $"Name {i}",
                    Surname = $"Surname {i}",
                    Age = i,
                    IsHuman = i % 2 == 0
                });
            }
            return users;
        }
    }
}
