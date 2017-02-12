using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;



namespace Applenium
{
    [TestFixture]
    class InfrustructureUnitTestsSSH
    {
        [Test]
        public void ssh_clien()
        {
            SshCilent ssh = new SshCilent();
            ssh.ssh_connect("10.10.90.163", "admin", "admin");
            ssh.ssh_command("uname -a");
            ssh.ssh_close();
            
        }
    }
}
