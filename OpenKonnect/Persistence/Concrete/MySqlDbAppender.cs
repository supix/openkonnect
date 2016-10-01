using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenKonnect.Persistence.Abstract;
using OpenKonnect.Domain;
using log4net;
using MySql.Data.MySqlClient;

namespace OpenKonnect.Persistence.Concrete
{
    public class MySqlDbAppender : IDbAppender
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly string connectionString;

        public MySqlDbAppender(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Insert(Timbratura t)
        {
            const string sqlString =
@"INSERT INTO stamps 
(STAMPTIME, DIRECTION, REASON, CARDCODE, READERCODE, INSERTIONTIME)
VALUES
(@STAMPTIME, @DIRECTION, @REASON, @CARDCODE, @READERCODE, @INSERTIONTIME)";

            log.Debug("Opening connection");
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                log.Debug("Connection open");

                using (var tx = conn.BeginTransaction())
                {
                    using (var com = conn.CreateCommand())
                    {
                        com.Transaction = tx;
                        com.CommandText = sqlString;

                        com.Parameters.AddWithValue("STAMPTIME", t.DateTime);
                        com.Parameters.AddWithValue("DIRECTION", t.VersoToChar());
                        com.Parameters.AddWithValue("REASON", t.Causale);
                        com.Parameters.AddWithValue("CARDCODE", t.CodiceInterno);
                        com.Parameters.AddWithValue("READERCODE", t.IdLettore);
                        com.Parameters.AddWithValue("INSERTIONTIME", DateTime.Now);

                        com.ExecuteNonQuery();
                        log.Debug("Item inserted");
                    }

                    tx.Commit();
                    log.Debug("Transaction committed");
                }
            }
            log.Debug("Connection closed");
        }
    }
}
