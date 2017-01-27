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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Renci.SshNet; /* reference needed: Renci.SshNet.dll */
using System.Diagnostics;
using ReadWriteIniFileExample;

namespace Installation_Check
  {
  public class truc
    {
    public static String StrToK(String Key, ref String S)
      {
      String Resultat;
      int I = S.IndexOf(Key);
      if (-1 == I)
        {
        Resultat = S;
        S = "";
        }
      else
        {
        Resultat = S.Substring(0, I);
        S= S.Remove(0, I + Key.Length);
        }

      return Resultat;
      }
    }

  public class TStringList : List<String>
    {
    public String Text()
      {
      return String.Join("\n", this);
      }
    public void _from_Split(String _s, params char[] separator)
      {
      base.AddRange(_s.Split(separator).ToList());
      }
    }
  public class TslCommande : List<TblCommande> { }
  public abstract class TblCommande
    {
    public String Commande { get; set; }
    public String Libelle { get; set; }
    public String Resultat { get; set; }
    public TStringList slResultat;
    public int ExitStatus { get; set; }
    public string Error { get; set; }
    public String LED_Color { get; set; }
    private TextBox tb;
    private StackPanel sp;

    public TblCommande(TslCommande _sl)
      {
      _sl.Add(this);
      LED_Color = "Black";
      slResultat = new TStringList();
      }
    //Initialisation
    public TblCommande Init(TextBox _tb, StackPanel _sp, String _Commande, String _Libelle)
      {
      tb = _tb;
      sp = _sp;
      Commande = _Commande;
      Libelle = _Libelle;
      if ("" == Libelle) Libelle = Commande;
      Accroche();
      return this;
      }
    protected TdkCommande uc = null;
    private void Accroche()
      {
      uc = new TdkCommande(this);
      sp.Children.Add(uc);
      }

    //Lancement du thread
    public abstract void Execute();
    public void Add_Line(String _S)
      {
      tb.Text += _S + "\r\n";
      }
    public void Display_Resultat()
      {
      if (ExitStatus != 0)
        {
        Add_Line("ExitStatus: >" + ExitStatus + "<");
        Add_Line("Error: >" + Error + "<");
        }
      string Resultat_Formatte = Resultat.Replace("\n", "\r\n");
      Add_Line("Resultat: >" + Resultat_Formatte + "<");
      }
    public virtual void Commande_Terminated()
      {
      LED_Color = "Lime";
      Libelle = Libelle + ": " + (slResultat.Count > 0 ? slResultat[0] : "");
      }
    }
  public class TblCommande_ssh : TblCommande
    {
    private SshClient ssh;
    public TblCommande_ssh(TslCommande _sl) : base(_sl) { }
    //Initialisation
    public TblCommande Init(SshClient _ssh, TextBox _tb, StackPanel _sp, String _Commande, String _Libelle)
      {
      ssh = _ssh;
      return base.Init(_tb, _sp, _Commande, _Libelle);
      }
    public override void Execute()
      {
      SshCommand c = ssh.CreateCommand(Commande);
      Resultat = c.Execute();
      ExitStatus = c.ExitStatus;
      Error = c.Error;
      slResultat._from_Split(Resultat, '\n');
      Commande_Terminated();
      }

    }
  public class TblCommande_locale : TblCommande
    {
    String Arguments;
    public TblCommande_locale(TslCommande _sl) : base(_sl) { }
    public TblCommande Init(TextBox _tb, StackPanel _sp, String _Commande, String _Arguments, String _Libelle)
      {
      Arguments = _Arguments;
      return base.Init(_tb, _sp, _Commande, _Libelle);
      }
    //Initialisation
    public override void Execute()
      {
      // Start the child process.
      Process p = new Process();
      // Redirect the output stream of the child process.
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.RedirectStandardError = true;
      //p.StartInfo.FileName = "YOURBATCHFILE.bat";
      p.StartInfo.FileName = Commande;
      p.StartInfo.Arguments = Arguments;
      p.Start();
      // Do not wait for the child process to exit before
      // reading to the end of its redirected stream.
      // p.WaitForExit();
      // Read the output stream first and then wait.
      p.WaitForExit();
      Resultat = p.StandardOutput.ReadToEnd();
      Error = p.StandardError.ReadToEnd();
      ExitStatus = p.ExitCode;
      slResultat._from_Split(Resultat, '\n');
      Commande_Terminated();
      }

    }
  public class TblCommande_Inifile : TblCommande
    {
    String IniFile, Section, Key, Value;
    public TblCommande_Inifile(TslCommande _sl) : base(_sl) { }
    public TblCommande Init(TextBox _tb, StackPanel _sp, String _IniFile, String _Section, String _Key, String _Value, String _Libelle)
      {
      IniFile = _IniFile;
      Section = _Section;
      Key = _Key;
      Value = _Value;

      return base.Init(_tb, _sp, "", _Libelle);
      }
    //Initialisation
    public override void Execute()
      {
      Resultat = IniFileHelper.ReadValue(Section, Key, IniFile);
      Error = "";
      ExitStatus = 0;
      slResultat._from_Split(Resultat, '\n');
      Commande_Terminated();
      if (Value != "")
        LED_Color = (Resultat == Value) ? "Lime" : "Red";
      }
    }
  }
