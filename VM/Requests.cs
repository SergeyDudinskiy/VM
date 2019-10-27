using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace VM
{
    public static class Requests
    {
        public static BindingSource Select(string command, ref SqlDataAdapter dataAdapter)
        {
            BindingSource bs = null;

            try
            {
                SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BD.mdf;Integrated Security=True");
                dataAdapter = new SqlDataAdapter(command, con);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                bs = new BindingSource(); //источник данных для таблицы на форме
                bs.DataSource = table;
            }
            catch
            {
                
            }

            return bs;           
        }        

        public static string Request(string request)
        {
            string errorMessage = "";
            SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BD.mdf;Integrated Security=True");

            try
            {
                con.Open();
                SqlCommand com = new SqlCommand(request, con);
                com.ExecuteNonQuery().ToString(); //получаем результат запроса                             
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
            }
            finally
            {
                con.Close();
            }

            return errorMessage;
        }

        public static List<int> GetList(string request)
        {
            List <int> list = null;           

            try
            {
                SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BD.mdf;Integrated Security=True");
                SqlDataAdapter adapter = new SqlDataAdapter(request, con);
                DataTable table = new DataTable();
                adapter.Fill(table);
                list = new List<int>();

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn c in table.Columns)
                    {
                        list.Add((int)row[c]);
                    }                                  
                }
            }
            catch 
            {
                
            }

            return list;      
        }       

        public static int GetValue(string request)
        {
            int value = -1;
            SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BD.mdf;Integrated Security=True");

            try
            {
                con.Open();
                SqlCommand com = new SqlCommand(request, con);
                value = (int)com.ExecuteScalar();                
            }
            catch
            {                
                value = -1;
            }
            finally
            {
                con.Close();
            }            

            return value;
        }
    }
}