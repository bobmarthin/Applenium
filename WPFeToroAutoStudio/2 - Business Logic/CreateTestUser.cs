using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Applenium._2___Business_Logic;
using AutomaticTest;
using ServiceStack;

namespace Applenium
{
    internal class CreateTestUser
    {

        public AppleniumLogger logger = new AppleniumLogger();
        public int AffId = 0;
        private const string UserApiUrl = "http://ta-cash-w12-02:81/api/v1/users";// uvo1pmjqbw7zu87luhm.vm.cld.sr:81/api/v1/users; /*ta-cash-w12-02:81/api/v1/users;*/
        public readonly string DbConnectionStrGlobalRegistry = ConfigurationManager.AppSettings["GlobalRegConnectionString"];
        public readonly string DbConnectionStrKyc = ConfigurationManager.AppSettings["KYCDBConnectionString"];
        private readonly string _dbConnectionStrFiktivo = ConfigurationManager.AppSettings["FiktivoDBQAConnectionString"];
        private readonly string _dbConnectionStrRealMirrorQa = ConfigurationManager.AppSettings["RealMirrorQAConnectionString"];


        public NewUser CreateUser(string userType, string input, string randomUserName)
            //affWiz,KYC usage (input is actually a verification level of the user: for affWiz input = 3, for KYC input = 0,1,2,3)
        {
            var client = new JsonServiceClient(UserApiUrl) {Timeout = TimeSpan.FromMinutes(30)};
            NewUser newUser = null;
            try
            {
                CreateUserApiRequest rr = new CreateUserApiRequest(randomUserName, "123456",
                    randomUserName+"@aijl.com", "123123123", "123123123123",
                    "asdasdasdasd", "asdasdasdasd", "jj", "France", "424242", "1966-6-6", "127.0.0.1", "1", 4, 42);
                var res = client.Post<CreateUserApiResponse>(UserApiUrl, rr);

                int gCid = res.gcid;
                int cidDemo = res.cidDemo;
                int cidReal = res.cidReal;
                newUser = new NewUser(gCid, cidDemo, cidReal, randomUserName, "123456");
                
            }
            catch (Exception e)
            {
                LogObject log = new LogObject();
                log.Description = "Create test user";
                log.StatusTag = Constants.ERROR;
                log.Exception = e;
                logger.Print(log);
                return null;
            }

            switch (userType)
            {
                case Constants.FtdUserNoAff:
                    if (!InitFtdUser(newUser, input))
                        return null; //could not update
                    break;
                case Constants.FtdUserWithAff:
                    string GetLastCreatedAffiliateIdCommand =
                        "select Top 1 AffiliateID from [FiktivoQA].[dbo].[tblaff_Affiliates] order by AffiliateID desc";
                    if (!InitFtdUser(newUser, input))
                        return null; //could not update

                    int affId =
                        GetLastCreatedAffiliateId(GetLastCreatedAffiliateIdCommand,
                            _dbConnectionStrFiktivo);
                    if (affId != 0)
                    {
                        AffId = affId;
                        string insertAffiliateCommand =
                            @"insert into [RealMirrorQA].[BackOffice].[Affiliate] " +
                            @"Values(" + affId + ", 1, 0, Null)";
                        if (UpdateFtdUser(insertAffiliateCommand,
                            _dbConnectionStrRealMirrorQa) != 1) //could not insert
                            return null;

                        string updateSerialIdCommand =
                            @"UPDATE [RealMirrorQA].[Customer].[Customer] " +
                            @"SET [SerialID] = " + affId +
                            @" WHERE [CID]  = " + newUser.Real_CID;


                        if (UpdateFtdUser(updateSerialIdCommand,
                            _dbConnectionStrRealMirrorQa) != 1) //could not insert
                            return null;
                    }
                    break;
					

                //    case Constants.FtdUserRespQuestionNoAff:
                //        int rowsChangedCount = 0;
                //        string insertResponsibilityAnswerCommand =
                //            @"INSERT INTO UserApiDB.KYC.CustomerAnswers (GCID, QuestionId, AnswerId, OccurredAt) " +
                //            @"VALUES (" + gCid + ", 7, 17, GETDATE() )";
                //        if (!InitFtdUser(newUser, input))
                //            return null; //could not update
                //        rowsChangedCount = UpdateFtdUser(insertResponsibilityAnswerCommand, _dbConnectionStrKyc);
                //        if (rowsChangedCount != 1) //could not insert
                //            return null;
                //        break;
            }

            return newUser;
        }
            

        public bool InitFtdUser(NewUser newUser, string input)
        {
            string verLevel = input;
            int rowsChangedCount = 0;
            bool userInited = true;
            //giving the new user enough verification to deposit  in cashier
            string updateVerificationLevelIdCommand =
                @"UPDATE [RealMirrorQA].[BackOffice].[Customer]" +
                @" SET [VerificationLevelID]  = " + verLevel +
                @" WHERE [CID]  = " + newUser.Real_CID;

            rowsChangedCount = UpdateFtdUser(updateVerificationLevelIdCommand, DbConnectionStrGlobalRegistry);

            if (rowsChangedCount != 1) //could not update
                userInited = false;
  
            return userInited;
        }

        public int UpdateFtdUser(string command, string conString)
        {

            int rows = 0;


            using (SqlConnection aconn = new SqlConnection(conString))
            {
                try
                {
                    aconn.Open();
                    using (SqlCommand cmd = new SqlCommand(command))
                    {
                        cmd.Connection = aconn;
                        rows = cmd.ExecuteNonQuery(); //rows number of record got updated
                    }
                }

                catch (Exception e)
                {
                    AppleniumLogger exceptionLogger = new AppleniumLogger();
                    LogObject log = new LogObject();
                    log.Description = "Create test user";
                    log.StatusTag = Constants.ERROR;
                    log.Exception = e;
                    logger.Print(log);
                }
                finally
                {
                    if ((aconn != null) && (aconn.State == ConnectionState.Open))
                        aconn.Close();
                }
            }
            return rows;
        }

        public static bool AddRemoveAmount(int amountToAddInCents, int CID)
        {
            bool result = true;
            SqlConnection connection = null;
            SqlCommand cmd = null;
            try
            {

                connection = new SqlConnection(ConfigurationManager.AppSettings["GlobalRegConnectionString"]);
                connection.Open();
                cmd = new SqlCommand("[dbo].[BillingAmountAdd_Real]");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connection;

                SqlParameter aparam;

                aparam = new SqlParameter();
                aparam.ParameterName = "@CID";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = CID;
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@CurrencyID";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = 1;
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@AccountUpdateTypeID";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = 1; //Compensation
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@Amount ";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = amountToAddInCents;
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@PositionID";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = null;
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@ManagerID ";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = 0; //Avner
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@Description";
                aparam.SqlDbType = SqlDbType.VarChar;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = "Automatic Test deposit";
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@PaymentID";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = null;
                cmd.Parameters.Add(aparam);

                aparam = new SqlParameter();
                aparam.ParameterName = "@CompensationReasonID";
                aparam.SqlDbType = SqlDbType.Int;
                aparam.Direction = ParameterDirection.Input;
                aparam.Value = 24; //-test internal
                cmd.Parameters.Add(aparam);

                cmd.CommandTimeout = 0;
                cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                Utilities.LogError(e);
                //FinServer.FinSrv.WriteErrorToLog(e.Message + " from CloseAllPositions()");
                result = false;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                    connection = null;
                }
            }

            return result;

        }


        public static int GetLastCreatedAffiliateId(string command, string conString)
        {
            int LastCreatedAffiliateId = 0;
            using (SqlConnection conn = new SqlConnection(conString))
            {
                
                SqlCommand cmd = new SqlCommand(command,conn);
                try
                {
                    conn.Open();
                    LastCreatedAffiliateId = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                }
                return LastCreatedAffiliateId;

            }
        }
    }
}
