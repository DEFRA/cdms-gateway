using CdmsGateway.Utils.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using NSubstitute;

namespace CdmsGateway.Test.Utils.Mongo
{
   public class MongoServiceTests
   {
      private readonly IMongoCollection<TestModel> _collectionMock;

      private readonly TestMongoService _service;

      public MongoServiceTests()
      {
         var connectionFactoryMock = Substitute.For<IMongoDbClientFactory>();
         Substitute.For<ILoggerFactory>();
         Substitute.For<IMongoClient>();
         _collectionMock = Substitute.For<IMongoCollection<TestModel>>();

         connectionFactoryMock
            .GetClient()
            .Returns(Substitute.For<IMongoClient>());

         connectionFactoryMock
            .GetCollection<TestModel>(Arg.Any<string>())
            .Returns(_collectionMock);

         _collectionMock.CollectionNamespace.Returns(new CollectionNamespace("test", "example"));
         _collectionMock.Database.DatabaseNamespace.Returns(new DatabaseNamespace("test"));


         _service = new TestMongoService(connectionFactoryMock, "testCollection", NullLoggerFactory.Instance);

         _collectionMock.DidNotReceive().Indexes.CreateMany(Arg.Any<IEnumerable<CreateIndexModel<TestModel>>>());
      }

      [Fact]
      public void EnsureIndexes_CreatesIndexes_WhenIndexesAreDefined()
      {
         var indexes = new List<CreateIndexModel<TestModel>>()
            {
                new(Builders<TestModel>.IndexKeys.Ascending(x => x.Name))
            };
         _service.SetIndexes(indexes);
         _service.RunEnsureIndexes();

         _collectionMock.Received(1).Indexes.CreateMany(indexes);
      }

      [Fact]
      public void EnsureIndexes_DoesNotCreateIndexes_WhenIndexesAreNotDefined()
      {
         _service.SetIndexes(new List<CreateIndexModel<TestModel>>());
         _service.RunEnsureIndexes();

         _collectionMock.DidNotReceive().Indexes.CreateMany(Arg.Any<IEnumerable<CreateIndexModel<TestModel>>>());

      }

      public class TestModel
      {
         public string Name { get; init; }
      }

      private interface ITestMongoService
      {
         public List<CreateIndexModel<TestModel>> GetIndexes();
         public void SetIndexes(List<CreateIndexModel<TestModel>> indexes);
      }
      
      private class TestMongoService : MongoService<TestModel>, ITestMongoService
      {
         private List<CreateIndexModel<TestModel>> _indexes = new();

         public TestMongoService(IMongoDbClientFactory connectionFactory, string collectionName, ILoggerFactory loggerFactory)
             : base(connectionFactory, collectionName, loggerFactory)
         {
         }

         public List<CreateIndexModel<TestModel>> GetIndexes()
         {
            return _indexes;
         }

         public void SetIndexes(List<CreateIndexModel<TestModel>> indexes)
         {
            _indexes = indexes;
         }

         protected override List<CreateIndexModel<TestModel>> DefineIndexes(IndexKeysDefinitionBuilder<TestModel> builder)
         {
            if (GetIndexes() == null)
            {
               throw new Exception("Indexes not defined");
            }
            return GetIndexes();
         }

         public void RunEnsureIndexes()
         {
            EnsureIndexes();
         }


      }

   }
}
