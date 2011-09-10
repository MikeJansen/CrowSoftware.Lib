using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace CrowSoftware.Common
{
    public static class DBUtility
    {
        public static DbConnection GetConnection(string configurationKey)
        {
            ConnectionStringSettings connectionStringSettings =
                ConfigurationManager.ConnectionStrings[configurationKey];

            DbProviderFactory factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionStringSettings.ConnectionString;
            return connection;
        }
    
        public static List<T> GetCollection<T>(DbCommand command) where T:IDataReaderInitializable, new()
        {
            List<T> list = new List<T>();

            try
            {
                command.Connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        T item = new T();
                        ((IDataReaderInitializable)item).Initialize(reader);
                        list.Add(item);
                    }
                }
            }
            finally
            {
                if (command.Connection.State == ConnectionState.Open)
                {
                    command.Connection.Close();
                }
            }

            return list;
        }

    }
}
