using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using PoGo_Proxy.Models;

namespace PoGo_Proxy.Logging
{
    public class MongoLogger : ILogger
    {

        static MongoLogger()
        {
            ConventionRegistry.Register(
                "DictionaryRepresentationConvention",
                new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays) },
                _ => true);
        }

        private readonly MongoClient _client;
        private readonly string _defaultDatabase;

        private MongoLogger() { }

        public MongoLogger(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            _defaultDatabase = MongoUrl.Create(connectionString).DatabaseName;
            _client = new MongoClient(connectionString);
        }

        public async Task Log(PoGoWebRequest webRequest)
        {
            var collection = GetCollection<PoGoWebRequest>();
            try
            {
                await collection.InsertOneAsync(webRequest);
            }
            catch (Exception)
            {
            }
        }

        private IMongoDatabase GetDatabase(string databaseName = null)
        {
            if (databaseName == null)
            {
                databaseName = _defaultDatabase;
            }
            var db = _client.GetDatabase(databaseName);
            return db;
        }

        private IMongoCollection<T> GetCollection<T>(string collectionName = null, string databaseName = null, bool slaveOk = false)
        {
            var db = GetDatabase(databaseName);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = GetTypeName(typeof(T));
            }
            MongoCollectionSettings settings = null;
            if (slaveOk)
            {
                settings = new MongoCollectionSettings { ReadPreference = ReadPreference.SecondaryPreferred };
            }
            return db.GetCollection<T>(collectionName, settings);
        }

        private static readonly ConcurrentDictionary<Type, string> TypeNames = new ConcurrentDictionary<Type, string>();

        private static string GetTypeName(Type type)
        {
            return TypeNames.GetOrAdd(type, (t) =>
            {
                var collectionName = t.Name;
                if (t.GenericTypeArguments.Length > 0)
                {
                    collectionName = collectionName.Remove(collectionName.IndexOf('`')) + "-" +
                                     t.GenericTypeArguments[0].Name;
                }
                return collectionName;

            });
        }

    }

    class DictionaryRepresentationConvention : ConventionBase, IMemberMapConvention
    {
        private readonly DictionaryRepresentation _dictionaryRepresentation;
        public DictionaryRepresentationConvention(DictionaryRepresentation dictionaryRepresentation)
        {
            _dictionaryRepresentation = dictionaryRepresentation;
        }
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.SetSerializer(ConfigureSerializer(memberMap.GetSerializer()));
        }
        private IBsonSerializer ConfigureSerializer(IBsonSerializer serializer)
        {
            var dictionaryRepresentationConfigurable = serializer as IDictionaryRepresentationConfigurable;
            if (dictionaryRepresentationConfigurable != null)
            {
                serializer = dictionaryRepresentationConfigurable.WithDictionaryRepresentation(_dictionaryRepresentation);
            }

            var childSerializerConfigurable = serializer as IChildSerializerConfigurable;
            return childSerializerConfigurable == null
                ? serializer
                : childSerializerConfigurable.WithChildSerializer(ConfigureSerializer(childSerializerConfigurable.ChildSerializer));
        }
    }
}