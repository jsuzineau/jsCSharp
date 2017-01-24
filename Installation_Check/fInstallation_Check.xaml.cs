/*                                                                              |
    Author: Jean SUZINEAU<Jean.Suzineau@wanadoo.fr>                             |
            http://www.mars42.com                                               |
                                                                                |
    Copyright 2016 Jean SUZINEAU - MARS42                                       |
                                                                                |
    This program is free software: you can redistribute it and/or modify        |
    it under the terms of the GNU Lesser General Public License as published by |
    the Free Software Foundation, either version 3 of the License, or           |
    (at your option) any later version.                                         |
                                                                                |
    This program is distributed in the hope that it will be useful,             |
    but WITHOUT ANY WARRANTY; without even the implied warranty of              |
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the                 |
    GNU Lesser General Public License for more details.                         |
                                                                                |
    You should have received a copy of the GNU Lesser General Public License    |
    along with this program.If not, see<http://www.gnu.org/licenses/>.          |
                                                                                |
|                                                                              */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet; /* reference needed: Renci.SshNet.dll */
using Installation_Check;
using CSScriptLibrary;
using System.IO;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Installation_Check
  {
  public partial class fInstallation_Check : Window
    {
    TInstallation_Check ic;
    String NomScript = "script.cs";
    String NomScriptConnexion = "scriptConnexion.cs";
    String ScriptConnexion;
    public fInstallation_Check()
      {
      InitializeComponent();
      ic= new TInstallation_Check( tb, sp);
      if (File.Exists(NomScript))
        tbScript.Text = File.ReadAllText(NomScript);
      else
        File.WriteAllText(NomScript, tbScript.Text);
      if (File.Exists(NomScriptConnexion))
        ScriptConnexion = File.ReadAllText(NomScriptConnexion);
      
      }

    private void bRun_Click(object sender, EventArgs e)
    {
      try
        {
        //script_classe();
        script_methode();
        }
      catch(Exception ex)
        {
        tb.Text = "Exception \n" + ex.Message + "\nPile d'appels:\n" + Environment.StackTrace;
        }
    }
  private void script_classe()
    {
    /*
    @"using System;
        public class Script
        {
            public static void SayHello()
            {
                Console.WriteLine(Host.greeting);
            }
        }"
    */
    AsmHelper scriptAsm = new AsmHelper(CSScript.LoadCode(tbScript.Text));

    scriptAsm.Invoke("Script.Execute", ic);
/*
namespace Installation_Check
  {
  public class Script
    {
    static void Execute(TInstallation_Check _ic)
      {
      _ic.Traite_Commande("pwd", "pwd");
      _ic.Verifie_CHMOD_777("./tmp/test_ll/droits_777");
      _ic.Verifie_CHMOD_777("./tmp/test_ll/droits_differents");
      _ic.Verifie_Owner_Group("./tmp/test_ll/non_partage", "jean", "jean");
      _ic.Verifie_Owner_Group("./tmp/test_ll/partage", "jean", "jean");
      _ic.Traite_Commande("fpc -v", "Version de FreePascal");
      //_ic.Traite_ll("./");
      }
    }
  }
*/
      }


    private void script_methode()
      {
      /*
        @"void Print(HostApp host){ Console.WriteLine(host.Name); }"
       */

      dynamic script = CSScript.Evaluator.LoadMethod(ScriptConnexion);
      script.Execute(ic);

      script = CSScript.Evaluator.LoadMethod(tbScript.Text);
      script.Execute(ic);
 
/*
using Installation_Check;
void Execute(TInstallation_Check _ic)
  {
  _ic.Connect("NomHôte", "NomUtilisateur","MotDePasse");
  }
*/
/*
using Installation_Check;
void Execute(TInstallation_Check _ic)
  {
  _ic.Traite_Commande("pwd", "pwd");
  _ic.Verifie_CHMOD_777("./tmp/test_ll/droits_777");
  _ic.Verifie_CHMOD_777("./tmp/test_ll/droits_differents");
  _ic.Verifie_Owner_Group("./tmp/test_ll/non_partage", "jean", "jean");
  _ic.Verifie_Owner_Group("./tmp/test_ll/partage", "jean", "jean");
  _ic.Traite_Commande("fpc -v", "Version de FreePascal");
  //_ic.Traite_ll("./");
  }
*/
    }

    private void fInstallation_Check_Shown(object sender, EventArgs e)
      {
      }

    private void tbScriptConnexion_TextChanged(object sender, TextChangedEventArgs e)
      {
      }

    private void Window_Initialized(object sender, EventArgs e)
      {

      }
    }
}
