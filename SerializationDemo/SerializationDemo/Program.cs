using System.IO;
using System.Xml.Serialization;
using System.Text.Json;

namespace SerializationDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            Person person = new()
            {
                Name = "John Doe",
                Age = 30,
                Address = "123 Main St",
                Password = "secret",
                Email = "helloworld@gmail.com"
            };

            string serializedPerson = Serialize(person);

            Console.WriteLine("Serialized Person:");
            Console.WriteLine(serializedPerson);

        }

        public static string Serialize(Person person)
        {
            if (person == null || string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Address) ||
                string.IsNullOrEmpty(person.Email) || string.IsNullOrEmpty(person.Password))
            {
                Console.WriteLine("Error: Person object is null or has missing required properties.");
                return string.Empty;
            }

            return JsonSerializer.Serialize(person);
        }
    }

    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Address { get; set; }

        public string? Password { get; set; } // This property will not be serialized

        public string? Email { get; set; } // This property will not be serialized

        public static string Encrypt(string input)
        {
            // Simple encryption logic (for demonstration purposes only)
            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }
    }
}