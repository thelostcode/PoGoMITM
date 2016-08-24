using System;
using System.Collections.Concurrent;
using System.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;

namespace PoGo_Proxy.MongoDB
{
    public static class MongoHelper
    {
        private static readonly MongoClient Client;
        private static readonly string DefaultDatabase;

        private static readonly ConcurrentDictionary<Type, string> TypeNames = new ConcurrentDictionary<Type, string>();

        static MongoHelper()
        {
            ConventionRegistry.Register("DictionaryRepresentationConvention", new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays) }, _ => true);

            var connectionString = ConfigurationManager.ConnectionStrings["mongodb"].ConnectionString;
            DefaultDatabase = MongoUrl.Create(connectionString).DatabaseName;
            Client = new MongoClient(connectionString);
        }

        private static IMongoDatabase GetDatabase(string databaseName = null)
        {
            if (databaseName == null)
            {
                databaseName = DefaultDatabase;
            }
            var db = Client.GetDatabase(databaseName);
            return db;
        }

        public static IMongoCollection<T> GetCollection<T>(string collectionName = null, string databaseName = null, bool slaveOk = false)
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

        public static void DropCollection<T>(string collectionName = null, string databaseName = null)
        {
            var db = GetDatabase(databaseName);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }
            db.DropCollection(collectionName);
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
