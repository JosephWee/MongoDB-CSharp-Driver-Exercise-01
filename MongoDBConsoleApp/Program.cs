using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace MongoDBConsoleApp
{
    class Program
    {
        public class Product
        {
            public ObjectId Id { get; set; }
            public int prod_id { get; set; }
            public double price { get; set; }
            public DateTime entryDateTime { get; set; }
        }

        static void Main(string[] args)
        {
            var connString =
                System
                .Configuration
                .ConfigurationManager
                .ConnectionStrings["MongoDB"];

            MongoClient dbClient = new MongoClient(connString.ConnectionString);

            var db = dbClient.GetDatabase("MyFirstMongoDB");

            UpdateProductPrice(db);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void UpdateProductPrice(IMongoDatabase db)
        {
            Random rand = new Random();

            var products = db.GetCollection<Product>("Products");

            for (int i = 1; i <= 1000; i++)
            {
                var product = new Product()
                {
                    prod_id = i,
                    price = 12 + ((double)rand.Next(-5, 5) / 10D),
                    entryDateTime = DateTime.UtcNow
                };

                products.InsertOne(product);
            }
        }
    }
}
