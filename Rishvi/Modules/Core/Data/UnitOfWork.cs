using System;
using System.Threading.Tasks;

namespace Rishvi.Modules.Core.Data
{
    public interface IUnitOfWork : IDisposable
    {
        SqlContext Context { get; }
        void Commit();
        Task CommitAsync();
    }

    public class UnitOfWork : Disposable, IUnitOfWork
    {
        public SqlContext Context { get; }

        public UnitOfWork(SqlContext dataContext)
        {
            Context = dataContext;
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        protected override void DisposeCore()
        {
            Context?.Dispose();
            base.DisposeCore();
        }

        public async Task CommitAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}