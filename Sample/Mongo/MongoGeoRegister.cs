using Geo_Search.Interface;
using Geo_Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Threading.Tasks;

namespace Mongo.Geo_Search
{
    public static class MongoGeoRegister
    {
        public static IServiceCollection RegisterMongoGeoServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1️ Read MongoDB connection string and database name
            var connectionString = configuration.GetConnectionString("MongoDb");
            var databaseName = configuration["MongoSettings:DatabaseName"] ?? "GeoDemo";

            // 2️ Register Mongo client and database
            services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            // 3️ Ensure Geo Index exists for PetShopDocument
            services.AddScoped<IMongoCollection<PetShopDocument>>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var collection = database.GetCollection<PetShopDocument>("petshop");

                // ایجاد ایندکس در صورت نبود
                EnsureGeoIndexExists(collection).GetAwaiter().GetResult();

                return collection;
            });

            // 4️ Register the Geo Search service
            services.AddScoped<IGeoSearch<PetShopDto>, MongoGeoSearch>();

            return services;
        }

        private static async Task EnsureGeoIndexExists(IMongoCollection<PetShopDocument> collection)
        {
            var indexList = await collection.Indexes.ListAsync();
            var indexes = await indexList.ToListAsync();

            bool hasGeoIndex = indexes.Any(idx =>
            {
                var doc = idx.GetElement("key").Value.AsBsonDocument;
                return doc.Names.Contains("location");
            });

            if (!hasGeoIndex)
            {
                var indexKeys = Builders<PetShopDocument>.IndexKeys.Geo2DSphere(x => x.Location);
                var indexModel = new CreateIndexModel<PetShopDocument>(indexKeys);
                await collection.Indexes.CreateOneAsync(indexModel);
            }
        }
    }
}
