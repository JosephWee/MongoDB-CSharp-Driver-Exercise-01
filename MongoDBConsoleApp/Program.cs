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

            var aggProducts =
                db.GetCollection<Product>("Products")
                .Aggregate()
                .Group(
                    new BsonDocument
                    {
                        { "_id", "$prod_id" },
                        { "average", new BsonDocument( "$avg", "$price" ) }
                    }
                )
                .Sort(new BsonDocument { { "_id", 1 } })
                .ToList();

            int totalCount = 0;
            int correctCount = 0;
            foreach (var product in aggProducts)
            {
                double totalPrice = 0D;
                //StringBuilder stringBuilder = new StringBuilder();

                var filter = Builders<Product>.Filter.Eq("prod_id", product[0]);
                var products =
                    db.GetCollection<Product>("Products")
                    .Find(filter)
                    .Sort(new BsonDocument { { "entryDateTime", 1 } })
                    .ToList();

                foreach (var prod in products)
                {
                    //stringBuilder.AppendLine(
                    //    string.Format(
                    //        "Price: {0:0.00}\tDate: {1:O}",
                    //        prod.price,
                    //        prod.entryDateTime
                    //    )
                    //);

                    totalPrice += prod.price;
                }

                double expected = Math.Round(Convert.ToDouble(product["average"]), 2);
                double actual = Math.Round(totalPrice / Convert.ToDouble(products.Count), 2);
                bool areEqual = expected == actual;

                Console.WriteLine(
                    string.Format(
                        "prod_id: {0}",
                        product[0]
                    )
                );

                ++totalCount;

                if (areEqual)
                {
                    ++correctCount;
                }
                else
                {
                    Console.WriteLine(
                        string.Format(
                            "Average Price\r\n\tExpected: {1:0.00}\r\n\tActual: {2:0.00}",
                            product[0],
                            expected,
                            actual
                        )
                    );
                }
            }

            Console.WriteLine(
                string.Format(
                    "Total: {0}, Correct: {1}",
                    totalCount,
                    correctCount
                )
            );

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
