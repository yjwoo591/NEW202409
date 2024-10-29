using System.Data;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Database;

namespace PC1databaseCreator.Common.Library.Database
{
    public class DatabaseCommand : IDatabaseCommand
    {
        public string CommandText { get; }
        public object? Parameters { get; }
        public CommandType Type { get; }

        private DatabaseCommand(string commandText, object? parameters = null, CommandType type = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException(nameof(commandText));

            CommandText = commandText;
            Parameters = parameters;
            Type = type;
        }

        public static DatabaseCommand CreateStoredProcedure(string procName, object? parameters = null)
            => new(procName, parameters, CommandType.StoredProcedure);

        public static DatabaseCommand CreateQuery(string sql, object? parameters = null)
            => new(sql, parameters, CommandType.Text);

        public static DatabaseCommand CreateTableDirect(string tableName, object? parameters = null)
            => new(tableName, parameters, CommandType.TableDirect);
    }
}