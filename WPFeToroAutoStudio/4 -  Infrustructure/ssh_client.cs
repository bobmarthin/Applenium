using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using System.Text.RegularExpressions;

namespace Applenium._4____Infrustructure
{
    class ssh_client
    {
        private string server;
        private string user;
        private string passw;
        private static SshClient client;


        public String run_cmd(string command)
        {
            String output = "";
            string connection = "";

            var j = new JsonParser();

            string[] data = Regex.Split(command, @"\s*==>\s*");
            connection = data[0];
            command = data[1];

            connection = j.replaceVariable(connection);
            command = j.replaceVariable(command);

            data = Regex.Split(connection, @"\s*::\s*");

            server = data[0];
            user = data[1];
            passw = data[2];
            try
            {
                client = new SshClient(server, user, passw);
                client.Connect();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            if (!(client.IsConnected))
            {

                return ("Error: SSH Connection Error\n");
            }

            string[] commands = Regex.Split(command, ",,");

            foreach (string cmd in commands)
            {
                string run_cmd = cmd + " 2>&1";

                var res = client.RunCommand(run_cmd);
                output += res.Result.ToString();
            }

            client.Disconnect();

            return output;
        }

    }
}
