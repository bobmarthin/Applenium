using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Applenium._4____Infrustructure;
using System.Text.RegularExpressions;

namespace Applenium
{
    [TestFixture]
    class InfrustructureUnitTests
    {
        [Test]
        public void ReadFromJsonParser()
        {
            var jsonParser = new JsonParser();
            var testingEnvironmnet = Constants.MemoryConf["TestingEnvironmentVersion"];
            Assert.IsNotNullOrEmpty(testingEnvironmnet);
        }

        [Test]
        public void WriteToJsonParser()
        {
            var jsonParser = new JsonParser();
            jsonParser.WriteJson("WaitElementExists", "22");
            var waitElementExists = Constants.MemoryConf["WaitElementExists"];
            Assert.AreEqual("22", waitElementExists);
            jsonParser.WriteJson("WaitElementExists", "30");
            waitElementExists = Constants.MemoryConf["WaitElementExists"];
            Assert.AreEqual("30", waitElementExists);
        }
        [Test]
        public void SSHtest()
        {
            var j = new JsonParser();
            var ssh = new ssh_client();

            string str = "$$ssh_connection = 10.10.90.229::dev::dev123456";
            Boolean res = j.VariableFunctions(str);

            string ssh_res = ssh.run_cmd("$$ssh_connection ==> ls -l /etc/,,uname -a");
            Console.Write(ssh_res);
        }
        [Test]
        public void ReplaceVariable()
        {
            var j = new JsonParser();
            Boolean res = j.AddConfigToMemory("");
            string a = @"aaaaaaaaaaaaaaaaaaaaa $$AppiumServerHost ~ $$app-package\ ~  bbbbbbbbbbbbb";
            a = j.replaceVariable(a);
            Console.Write(a);
        }

        [Test]
        public void VariableFunctionsCreateVar()
        {
            var j = new JsonParser();
            string str = "$$test127 = AppiumServerHost";
            Boolean res = j.VariableFunctions(str);
            if (res)
            {
                str = "$$test127 .= SUPER";
                res = j.VariableFunctions(str);

                if (res)
                {
                    str = "$$test127 = $$test127 2 56 $$test127";
                    res = j.VariableFunctions(str);

                    if (res)
                    {
                        str = "$$test127 ~=  AppiumServerHost==>XXXX";
                        res = j.VariableFunctions(str);
                    }
                    if (res)
                    {
                        str = @"$$test127 ~= \d+";
                        res = j.VariableFunctions(str);
                    }
                }
            }

            
        }
        [Test]
        public void VariableFunctionsMath()
        {
            var j = new JsonParser();
            string str = "$$test127 = 200";
            Boolean res = j.VariableFunctions(str);
            if (res)
            {
                str = "$$test127 /= 2";
                res = j.VariableFunctions(str);

                Assert.AreEqual(true, res);
            }
            if (res)
            {
                str = "$$test127 *= 2";
                res = j.VariableFunctions(str);
            }
            if (res)
            {
                str = "$$test127 -=100";
                res = j.VariableFunctions(str);
            }
            if (res)
            {
                str = @"$$test127 += 100";
                res = j.VariableFunctions(str);
            }
            if (res)
            {
                str = @"$$test127 %= 3";
                res = j.VariableFunctions(str);
            }
        }
        //c=$$DataSource1 t=$$InitialDataTable cmd=select $$col from $$table $$db_serch
        [Test]
        public void sql_select()
        {
            var j = new JsonParser();
            var sql = new Sql();
            j.VariableFunctions("$$DataSource = Data Source=isr-sr-beaver;Integrated Security=True");
            j.VariableFunctions("$$InitialDataTable = Initial Catalog=QA_Autotest");
            j.VariableFunctions("$$db_cmd = select * from [QA_Autotest].[Test].[GuiTagType]");

            string line = @"s=$$DataSource t=$$InitialDataTable c=$$db_cmd";
            string db_out = sql.ExecuteCMDQuery(line);
            string[] data = Regex.Split(db_out, @"\n");

            foreach (string cmd in data)
            {
                string run_cmd = cmd ;
            }

        }
        [Test]
        public void sql_insert()
        {
            
            var j = new JsonParser();
            var sql = new Sql();
            j.VariableFunctions("$$DataSource = Data Source=isr-sr-beaver;Integrated Security=True");
            j.VariableFunctions("$$InitialDataTable = Initial Catalog=QA_Autotest");
            j.VariableFunctions("$$db_cmd = INSERT INTO QA_Autotest.Test.GuiTagType(TagType) values('cmd')");

            string line = @"s=$$DataSource t=$$InitialDataTable c=$$db_cmd";
            string db_out = sql.ExecuteCMDQuery(line);

        }
        [Test]
        public void sql_update()
        {

            var j = new JsonParser();
            var sql = new Sql();
            j.VariableFunctions("$$DataSource = Data Source=isr-sr-beaver;Integrated Security=True");
            j.VariableFunctions("$$InitialDataTable = Initial Catalog=QA_Autotest");
            j.VariableFunctions("$$db_cmd = UPDATE QA_Autotest.Test.GuiTagType SET TagType='vsh' WHERE  TagType='cmd'");

            string line = @"s=$$DataSource t=$$InitialDataTable c=$$db_cmd";
            string db_out = sql.ExecuteCMDQuery(line);

        }
        [Test]
        public void sql_delete()
        {

            var j = new JsonParser();
            var sql = new Sql();
            j.VariableFunctions("$$DataSource = Data Source=isr-sr-beaver;Integrated Security=True");
            j.VariableFunctions("$$InitialDataTable = Initial Catalog=QA_Autotest");
            j.VariableFunctions("$$db_cmd = DELETE FROM QA_Autotest.Test.GuiTagType  WHERE TagType='vsh'");

            string line = @"s=$$DataSource t=$$InitialDataTable c=$$db_cmd";
            string db_out = sql.ExecuteCMDQuery(line);

        }
        [Test]
        public void http_client()
        {

            var j = new JsonParser();
            //string url = "https://fapi-real.etoro.com/instruments.json";
            string url = "https://fapi-real.etoro.com/instruments/1";
            //string dbapiResult = HttpRequestExtensions.GetHTTPrequest(url);

            j.AddKeyToMemory(Constants.Memory, HttpRequestExtensions.GetHTTPrequest(url));


            string line = @"$$instrumentData j= InstrumentName";
            j.VariableFunctions(line);

            string str = j.replaceVariable("$$instrumentData");


            j.VariableFunctions("$$DataSource = " + str);

            str = j.replaceVariable("$$memory");

            line = @"$$instrumentData j= InstrumentName ==> $$DataSource";
            j.VariableFunctions(line);
            str = j.replaceVariable("$$instrumentData");
            
        }
        [Test]
        public void http_client_adv()
        {

            var j = new JsonParser();
           
            string url = "https://openbookmobile.etoro.com/api/login";
            string header = @"Content-Type::application/x-www-form-urlencoded; charset=utf-8,, User-Agent::OpenBook/2.1 (iPad Simulator; iOS 7.1; Scale/2.00)";
            string mode = "POST";
            string postdata = @"username=socialalert15&Password=123456m&appName=iOS&ver=2.1";
            string str = "m=" + mode + " h=" + header + " u=" + url + " p=" + postdata;


            //string dbapiResult = HttpRequestExtensions.GetHTTPrequest(url);

            j.AddKeyToMemory(Constants.Memory, HttpRequestExtensions.GetHTTPrequestPost(str));

            string line = @"$$Token j= Token";
            j.VariableFunctions(line);

            str = j.replaceVariable("$$Token");
            line = "https://openbookmobile.etoro.com/api/market?username=socialalert15&symbol=GOOG&appName=iOS&ver=2.1&period=180&code=en-gb&token=$$Token";
            j.AddKeyToMemory(Constants.Memory, HttpRequestExtensions.GetHTTPrequest(line));
            line = @"$$MarketValue j= MarketValue";
            j.VariableFunctions(line);
            str = j.replaceVariable("$$MarketValue");

            line = @"$$StockSummary j= StockSummary";
            j.VariableFunctions(line);
            str = j.replaceVariable("$$StockSummary");
        }
    }
}
