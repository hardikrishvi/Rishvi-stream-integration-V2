using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Rishvi.Modules.Core.Data.Seed
{
    public abstract class BaseSeed : IComparable<BaseSeed>
    {
        public int OrderId { get; set; }

        protected readonly SqlContext Context;

        public abstract void Seed();

        protected BaseSeed(SqlContext context)
        {
            Context = context;
        }

        public int CompareTo(BaseSeed other)
        {
            return OrderId.CompareTo(other.OrderId);
        }

        protected bool IsExistsTable(string tableName)
        {
            using var command = Context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "IF EXISTS (SELECT 1 FROM sys.tables WHERE [name] = '" + tableName + "') " +
                "BEGIN  " +
                "	SELECT 1 " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "	SELECT 0 " +
                "END;";
            Context.Database.OpenConnection();
            bool result = (int)command.ExecuteScalar() == 1;
            if (!result)
            {
                Console.WriteLine("");
                Console.WriteLine("===> Start Spinx Comment");
                Console.WriteLine("=> This table '" + tableName + "' does not exists into Database.");
                Console.WriteLine("=> Please again execute Package Manager Console command for seed data: update-database");
                Console.WriteLine("===> End Spinx Comment");
                Console.WriteLine("");
            }

            return result;
        }
        protected static string ReadFile(string moduleName, string fileName)
        {
            return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), $@"Modules\{moduleName}\Data\Seed\Templates", fileName));
        }
    }
}