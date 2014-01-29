using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticTest;

namespace Applenium
{
    class CreateTestUser
    {
        private string _dbConnectionStrGlobalRegistry = ConfigurationManager.AppSettings["GlobalRegConnectionString"];

        public NewUser CreateUser(string userType)
        {

            var loginRequest = new LoginRequest();
            var aconn = new SqlConnection(_dbConnectionStrGlobalRegistry);
            NewUser newUser = null;
            using (var acomm = new SqlCommand())
            {
                acomm.Connection = aconn;
                acomm.CommandType = CommandType.StoredProcedure;
                SqlParameter aparam = new SqlParameter();
                if (userType.Equals(Constants.FtdUser))
                {
                    aparam.ParameterName = "@DepositAmount";
                    aparam.SqlDbType = SqlDbType.Int;
                    aparam.Direction = ParameterDirection.Input;
                    aparam.Value = 0;    //no credit for this user because FTD is to be checked
                    acomm.Parameters.Add(aparam);
                    
                }
                acomm.CommandText = "[dbo].[CreateTestCustomer]";
               
                try
                {
                    aconn.Open();
                    SqlDataReader df = acomm.ExecuteReader();
                    if (df.Read()) // only returns one row so  use "if" instead of "while"
                    {
                        int gCid = df.GetInt32(0);
                        int cidDemo = df.GetInt32(1);
                        int cidReal = df.GetInt32(2);
                        string userName = df.GetString(3);
                         newUser = new NewUser(gCid, cidDemo, cidReal, userName, "123456");

                        if ((aconn != null) && (aconn.State == ConnectionState.Open))
                            aconn.Close();
                        switch (userType)
                        {
                            case Constants.OpenBookUser:
                                loginRequest.FirstUserLogin(newUser.UserName, newUser.Password);
                                break;
                            case Constants.FtdUser:
                                 int rowsChangedCount = 0;
                                //giving the new user enough verification to deposit  in cashier
                                string updateVerificationLevelIdCommand = 
                                @"UPDATE [RealMirrorQA].[BackOffice].[Customer]" +
                                @"SET [VerificationLevelID]  = 3 " +
                                @"WHERE [CID]  = " + newUser.Real_CID;
                                rowsChangedCount=InitFtdUser(updateVerificationLevelIdCommand);
                                if (rowsChangedCount != 1) //could not update
                                    return null;
                               
                            
                                //giving the new user an affiliate 
                                string updateSerialIdCommand = 
                                @"UPDATE [RealMirrorQA].[Customer].[Customer] " +
                                @"SET [SerialID] = 41421 " +
                                @"WHERE [CID]  = " + newUser.Real_CID;
                                rowsChangedCount = InitFtdUser(updateSerialIdCommand);
                                if (rowsChangedCount != 1) //could not update
                                    return null;

                                break;
                        }                                              
                    }
                }

                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                finally
                {
                    if ((aconn != null) && (aconn.State == ConnectionState.Open))
                        aconn.Close();
                }
            }

            return newUser;
        }

        private  int InitFtdUser(string command)
        {
            int rows = 0;

            using (SqlConnection aconn = new SqlConnection(_dbConnectionStrGlobalRegistry))
            {
                try
                {
                    aconn.Open();
                    using (SqlCommand cmd = new SqlCommand(command))
                    {
                        cmd.Connection = aconn;
                        rows = cmd.ExecuteNonQuery();//rows number of record got updated
                    }
                }

                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                finally
                {
                    if ((aconn != null) && (aconn.State == ConnectionState.Open))
                        aconn.Close();
                }
            }
            return rows;
        }

    }
}
