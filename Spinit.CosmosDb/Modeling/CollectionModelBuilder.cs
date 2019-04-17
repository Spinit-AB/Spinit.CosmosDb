using System;

namespace Spinit.CosmosDb
{
    public class CollectionModelBuilder<TEntity>
        where TEntity : ICosmosEntity
    {
        private CollectionModel _collectionModel { get; }

        internal CollectionModelBuilder(CollectionModel collectionModel)
        {
            _collectionModel = collectionModel;
        }

        public CollectionModelBuilder<TEntity> CollectionId(string collectionId)
        {
            _collectionModel.CollectionId = collectionId;
            return this;
        }

        public CollectionModelBuilder<TEntity> Analyzer(Action<AnalyzerBuilder> configure)
        {
            var builder = new AnalyzerBuilder(_collectionModel.Analyzer);
            configure(builder);
            _collectionModel.Analyzer = builder.Build();
            return this;
        }

        public CollectionModelBuilder<TEntity> Analyzer<TAnalyzer>()
            where TAnalyzer : Analyzer, new()
        {
            var analyzer = (Analyzer)Activator.CreateInstance<TAnalyzer>();
            _collectionModel.Analyzer = analyzer;
            return this;
        }

        public CollectionModelBuilder<TEntity> Analyzer<TAnalyzer>(TAnalyzer analyzer)
            where TAnalyzer : Analyzer
        {
            _collectionModel.Analyzer = analyzer;
            return this;
        }

        internal CollectionModel Build()
        {
            return _collectionModel;
        }
    }
}
