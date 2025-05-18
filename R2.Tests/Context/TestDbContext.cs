using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using R2.Data.Context;
using R2.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace R2.Tests.Context
{
    public class TestDbContext : R2DbContext
    {
        public DbSet<Category> TestCategories { get; set; }
        public DbSet<Resource> TestResources { get; set; }

        public TestDbContext(DbContextOptions<R2DbContext> options) : base(options)
        {
            // Initialiser avec des DbSets mockés
            TestCategories = CreateMockDbSet<Category>(new List<Category>()).Object;
            TestResources = CreateMockDbSet<Resource>(new List<Resource>()).Object;
        }

        // Pour initialiser avec des données spécifiques
        public void SetupTestData(List<Category> categories, List<Resource> resources = null)
        {
            TestCategories = CreateMockDbSet(categories).Object;
            if (resources != null)
            {
                TestResources = CreateMockDbSet(resources).Object;
            }
        }

        // Surcharger les propriétés pour retourner nos DbSets mockés
        public override DbSet<Category> Categories => TestCategories;
        public override DbSet<Resource> Ressources => TestResources;

        // Méthode utilitaire pour créer un mock DbSet qui supporte les opérations asynchrones
        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            // Setup pour IQueryable
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Setup pour les méthodes Add/Find
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);
            mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(items => data.AddRange(items));
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(item => data.Remove(item));

            // Setup pour ToListAsync
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            return mockSet;
        }

        // Implémentation de SaveChangesAsync
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1); // Simuler le succès avec 1 objet modifié
        }
    }

    // Classes de support pour les opérations asynchrones
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute))
                .MakeGenericMethod(resultType);

            var result = executeMethod.Invoke(_inner, new object[] { expression });

            var taskResult = typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { result });

            return (TResult)taskResult;
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IQueryProvider _provider;

        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
            _provider = new TestAsyncQueryProvider<T>(enumerable.AsQueryable().Provider);
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
            _provider = new TestAsyncQueryProvider<T>(new EnumerableQuery<T>(expression).AsQueryable().Provider);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => _provider;
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}