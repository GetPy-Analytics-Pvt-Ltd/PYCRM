using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Dapper;
using System.Configuration;
using System.IO;

namespace PYCRM
{
    class BackEnd
    {

        static string  logFilePathName;
        static string logFileName;
        static string logFileDir;
        static string CustTable = ConfigurationManager.AppSettings["DetailsTable"];
        static string StTable = ConfigurationManager.AppSettings["StateTable"];
        static string CtTable = ConfigurationManager.AppSettings["CityTable"];
        static string ArTable = ConfigurationManager.AppSettings["AreaTable"];
        //Generate Connection
        private static string LoadConnectionString(string id = "connection")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        //Load ComboBoxes that need to be fetched from the InfoConfig Database
        public static HashSet<string> LoadComboBox(string field,string Table)
        {
            HashSet<string> Itemlist = new HashSet<string>();
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sqlQuery = "SELECT "+field+" from "+ Table ;
                    SQLiteCommand cmd = new SQLiteCommand(sqlQuery, cnn);
                    SQLiteDataReader datareader = cmd.ExecuteReader();
                    while (datareader.Read())
                    {
                        string ItemName = datareader.GetString(0);
                        if(!(ItemName==null || ItemName==""))
                        {
                            Itemlist.Add(ItemName);
                        }
                    }
                }                
                return Itemlist;
            }
            catch(Exception ex)
            {
                return Itemlist;
            }
        }
        //To Delete a single Record From View Wondow
        public static void DeleteRecord(string Field ,string FieldToDelete)
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                string sqlQuery = "delete from "+ CustTable +" where "+ Field + " = '" + FieldToDelete+"'";
                SQLiteCommand cmd = new SQLiteCommand(sqlQuery, cnn);
                cmd.ExecuteNonQuery();
            }
        }
        //Function to load Customer Table to DataGrid in View Window To ShowCase to the Client
        public static DataTable LoadDataTableForDataGrid()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sqlQuery = "SELECT * from "+ CustTable +"";
                    SQLiteCommand cmd = new SQLiteCommand(sqlQuery, cnn);
                    SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                    dt.Clear();
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }

        public static void WriteToDataBase(CustomerBluePrint person)
        {
            try
            {
                bool CustomerExists = false;
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sqlQuery = "SELECT * from " + CustTable + " where number = '" + person.MobileNumber + "'";
                    SQLiteCommand cmd = new SQLiteCommand(sqlQuery, cnn);
                    SQLiteDataReader datareader = cmd.ExecuteReader();
                    if (datareader.HasRows)
                    {
                        CustomerExists = true;
                    }
                    else
                    {
                        CustomerExists = false;
                    }
                    datareader.Close();
                }
                if (CustomerExists)
                        ModifyAndSubmitToDatabase(person);
                    else
                        SubmitToDatabase(person);
                
            }
            catch(Exception ex)
            {
                Log("Write Error: " + ex.Message);
            }
        }

        //Function to midify Customer Details in local Database InfoConfig
        public static void ModifyAndSubmitToDatabase(CustomerBluePrint person)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    string sqlQry = "UPDATE " + CustTable + " SET "
                        + "name= @Name"
                        + ",email= @Email"
                        + ",gender= @Gender"
                        + ",birthday= @BirthDay"
                        + ",birthmonth= @BirthMonth"
                        + ",annivday= @AnniversaryDay"
                        + ",annivmonth= @AnniversaryMonth"
                        + ",agegroup= @AgeGroup"
                        + ",ethnicity= @Ethnicity"
                        + ",area= @Area"
                        + ",pin= @PinCode"
                        + ",city= @City"
                        + ",state= @State"
                        + ",referredby= @ReferredBy"
                        + ",remark= @Remark"
                        + ",dtModify = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "' WHERE number= '" + person.MobileNumber + "' ";
                    cnn.Execute(sqlQry, person);
                }
                if (person.Area != "")
                    UpdateDefaultListTable(ArTable, "area", person.Area);
                if (person.City != "")
                    UpdateDefaultListTable(CtTable, "city", person.City);
                if (person.State != "")
                    UpdateDefaultListTable(StTable, "state", person.State);
            }
            catch (Exception ex)
            {
                Log("Modification Error: " + ex.Message);
            }
        }



        public static void SubmitToDatabase(CustomerBluePrint person)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    string sqlQry ="insert into "+ CustTable +" (name,number,email,gender,birthday,birthmonth,annivday,annivmonth,agegroup,ethnicity,area,pin,city,state,referredby,remark,dtcreated)" +
                        " values (@Name,@MobileNumber,@Email,@Gender,@BirthDay,@BirthMonth,@AnniversaryDay,@AnniversaryMonth,@AgeGroup,@Ethnicity,@Area,@PinCode,@City,@State,@ReferredBy,@Remark,'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "')";
                    cnn.Execute(sqlQry, person);
                }
                if (person.Area != "")
                    UpdateDefaultListTable(ArTable, "area", person.Area);
                if (person.City != "")
                    UpdateDefaultListTable(CtTable, "city", person.City);
                if (person.State != "")
                    UpdateDefaultListTable(StTable, "state", person.State);
            }
            catch(Exception ex)
            {
                Log("Saving To DataBase Error: " + ex.Message);
            }
        }
        public static void UpdateDefaultListTable(string Table,string Field, string Value)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    string query = "INSERT INTO " + Table + " (" + Field + ") SELECT * FROM(SELECT '" + Value + "') AS tmp WHERE NOT EXISTS(SELECT * FROM " + Table + " WHERE " + Field + " = '" + Value + "')";
                    cnn.Execute(query);
                }
            }
            catch(Exception ex)
            {
                Log("BackEnd Error 101: "+ ex.Message);
            }
        }
        public static DataTable SearchDetailsByField(string Field, string FieldValue)
        {
            DataTable dt = new DataTable("Customer");
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sqlQuery = "SELECT* FROM "+ CustTable +" WHERE " + Field + "  LIKE '%" + FieldValue + "%'";
                    SQLiteCommand cmd = new SQLiteCommand(sqlQuery, cnn);
                    SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                    sda.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Log("Search Error: " + ex.Message);
                return dt;
            }
        }
        public static CustomerBluePrint GetDetailsByNumberToCustomerObject(string Number)
        {
            CustomerBluePrint customer = new CustomerBluePrint();
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sqlQuery = "SELECT * from "+ CustTable +" where number = '"+Number+"'";
                    SQLiteCommand cmd = new SQLiteCommand(sqlQuery, cnn);
                    SQLiteDataReader datareader = cmd.ExecuteReader();
                    if(datareader.HasRows)
                    {
                        while (datareader.Read())
                        {
                            customer.Name = (string)datareader["name"].ToString();
                            customer.MobileNumber = (string)datareader["number"].ToString();
                            customer.Email = (string)datareader["email"].ToString();
                            customer.Gender = (string)datareader["gender"].ToString();
                            customer.AgeGroup = (string)datareader["agegroup"].ToString();
                            customer.Area = (string)datareader["area"].ToString();
                            customer.City = (string)datareader["city"].ToString();
                            customer.State = (string)datareader["state"].ToString();
                            customer.PinCode = (string)datareader["pin"].ToString();
                            customer.BirthDay = (string)datareader["birthday"].ToString();
                            customer.BirthMonth = (string)datareader["birthmonth"].ToString();
                            customer.AnniversaryDay = (string)datareader["annivday"].ToString();
                            customer.AnniversaryMonth = (string)datareader["annivmonth"].ToString();
                            customer.Ethnicity = (string)datareader["ethnicity"].ToString();
                            customer.ReferredBy = (string)datareader["referredby"].ToString();
                            customer.Remark = (string)datareader["remark"].ToString();
                            customer.CreationDate = (string)datareader["dtcreated"].ToString();
                            customer.ModificationDates = (string)datareader["dtmodify"].ToString();
                        }
                        return customer;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log("BackEnd Error 102: "+ ex.Message);
                return null;
            }
        }
        public static void Log(string msg)
        {
            try
            {
                logFileName = ConfigurationManager.AppSettings["logfilename"];
                logFileDir = ConfigurationManager.AppSettings["logFileDir"];
                logFilePathName = Path.Combine(logFileDir, logFileName);
                using (var sw = new StreamWriter(logFilePathName, true))
                {

                    sw.Write(string.Format("{0} - {1}\n", DateTime.Now, msg));
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }
    }
}
