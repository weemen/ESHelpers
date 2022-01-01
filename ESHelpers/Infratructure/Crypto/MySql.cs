using ESHelpers.Encryption;
using MySql.Data.MySqlClient;

namespace ESHelpers.Infratructure.Crypto
{
    public class MySql : ICryptoStore
    {
        private string _connectionString;

        public MySql(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public EncryptionKey? loadKeyFromStore(string identifier)
        {
            var connection = new MySqlConnection(this._connectionString);
            connection.Open();
            var cmd = new MySqlCommand("SELECT `key`, `vector` FROM `keystore` WHERE `identifier` = @identifier", connection);
            cmd.Parameters.AddWithValue("@identifier", identifier);
            cmd.Prepare();
            
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                long startIndex = 0;
                var keyData = new byte[(int) 32]; 
                var vectorData = new byte[(int) 16];

                reader.GetBytes(0, startIndex, keyData,0, 32);
                reader.GetBytes(1, startIndex, vectorData,0, 16);
                connection.Close();
                return new EncryptionKey(keyData, vectorData);
            }
            connection.Close();
            return null;
        }

        public void SaveKeyToStore(string identifier, EncryptionKey encryptionKey)
        {
            var connection = new MySqlConnection(this._connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO `keystore` VALUES (@identifier, @key, @vector)";
            cmd.Parameters.AddWithValue("@identifier", identifier);
            cmd.Parameters.AddWithValue("@key", encryptionKey.Key);
            cmd.Parameters.AddWithValue("@vector", encryptionKey.Vector);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            connection.Close();
        }

        public void RemoveKeyFromStore(string identifier)
        {
            var connection = new MySqlConnection(this._connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM `keystore` WHERE `identifier` = @identifier";
            cmd.Parameters.AddWithValue("@identifier", identifier);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
}