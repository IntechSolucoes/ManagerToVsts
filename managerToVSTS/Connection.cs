using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace managerToVSTS
{
    public class Connection
    {
        public void GravarRegistroTaskNoBanco(int idTask, string tipo)
        {
            using (SqlConnection connectionString = new SqlConnection("Data Source=192.168.1.40; Initial Catalog = Lifesys; Persist Security Info = True; User ID = lifesys; Password = senha"))
            {
                var tabela = "";
                connectionString.Open();
                switch (tipo)
                {
                    case "P":
                        tabela = "ProductBacklogVSTS";
                        break;

                    case "B":
                        tabela = "BugVSTS";
                        break;
                }
                string sql = $"INSERT INTO " + tabela + " Values (" + idTask + ")";
                using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sql, connectionString))
                {
                    DataTable dataTable = new DataTable();
                    sqlAdapter.Fill(dataTable);
                }
            }
        }

        public void BuscarRegistroProductBacklog(int? idVsts)
        {
            Console.WriteLine("Buscando Product Backlog");
            using (SqlConnection connectionString = new SqlConnection("Data Source=192.168.1.40; Initial Catalog = Lifesys; Persist Security Info = True; User ID = lifesys; Password = senha"))
            {
                connectionString.Open();

                string sql = $"Select Protocolo from ProductBacklogVSTS where IdVsts = " + idVsts + " and Status <> " + 1 ;
                SqlCommand command = new SqlCommand(sql, connectionString);
                SqlDataReader reader;
                
                int protocolo = 0;

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    protocolo = Convert.ToInt16(reader["Protocolo"]);
                }
                reader.Close();
                command.Dispose();

                if (protocolo != 0)
                {
                    string sql2 = $"execute PROC$FINALIZA_VSTS {protocolo} ";
                    command = new SqlCommand(sql2, connectionString);
                    reader = command.ExecuteReader();
                    reader.Close();
                    command.Dispose();

                    string sql3 = $"update ProductBacklogVSTS set Status = 1 where Protocolo = {protocolo} ";
                    command = new SqlCommand(sql3, connectionString);
                    reader = command.ExecuteReader();
                    reader.Close();
                    command.Dispose();
                }
                
                connectionString.Close();
                connectionString.Dispose();                
            }
        }

        public void BuscarRegistroBug(int? idVsts)
        {
            Console.WriteLine("Buscando Bug");
            using (SqlConnection connectionString = new SqlConnection("Data Source=192.168.1.40; Initial Catalog = Lifesys; Persist Security Info = True; User ID = lifesys; Password = senha"))
            {
                connectionString.Open();

                string sql = $"Select Protocolo from BugVsts where IdVsts = " + idVsts + " and Status <> " + 1;
                SqlCommand command = new SqlCommand(sql, connectionString);
                SqlDataReader reader;

                int protocolo = 0;

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    protocolo = Convert.ToInt16(reader["Protocolo"]);
                }
                reader.Close();
                command.Dispose();

                if (protocolo != 0)
                {
                    string sql2 = $"execute PROC$FINALIZA_VSTS {protocolo} ";
                    command = new SqlCommand(sql2, connectionString);
                    reader = command.ExecuteReader();
                    reader.Close();
                    command.Dispose();

                    string sql3 = $"update BugVsts set Status = 1 where Protocolo = {protocolo} ";
                    command = new SqlCommand(sql3, connectionString);
                    reader = command.ExecuteReader();
                    reader.Close();
                    command.Dispose();
                }

                connectionString.Close();
                connectionString.Dispose();
            }
        }
    }
}
