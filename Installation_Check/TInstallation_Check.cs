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
using System.Drawing;
using Renci.SshNet; /* reference needed: Renci.SshNet.dll */
using System.Windows.Controls;

namespace Installation_Check
  {
  class TTraite_ll : TblCommande_ssh
    {
    //Attributs
    public String Repertoire;
    public TTraite_ll(TslCommande _sl) : base(_sl)
      {
      slConsolidation = new TStringList();
      }

    public TTraite_ll Init(SshClient _ssh, TextBox _tb, StackPanel _sp, String _Repertoire)
      {
      Repertoire = _Repertoire;
      base.Init(_ssh, _tb, _sp,
                 "ls -l -R " + Repertoire,
                 "Consolidation droits ll sur " + Repertoire);
      return this;
      }
    public override void Commande_Terminated()
      {
      base.Commande_Terminated();
      TStringList slBrut = new TStringList();
      slBrut._from_Split( Resultat, '\n');
      //Suppression du prompt à la fin
      //while
      //         (slBrut.Count>0)
      //     && (slBrut[slBrut.Count-1] = th.Prompt)
      //  slBrut.Delete( slBrut.Count-1); //le prompt

      if (slBrut.Count > 1) slBrut.RemoveAt(1); //total
      if (slBrut.Count > 0) slBrut.RemoveAt(0); //l'écho de la commande
      for (int i = slBrut.Count - 1; i >= 0; i--)
        {
        String s = slBrut[i];

        if (1 == s.IndexOf(Repertoire)) slBrut.RemoveAt(i);
        else if (1 == s.IndexOf("total")) slBrut.RemoveAt(i);
        else if ("" == s) slBrut.RemoveAt(i);
        }
      /*
      drwxrwxr-x  3 jean jean    4096 janv.  9  2016 analyseur_4gl
      */
      for (int i = 0; i < slBrut.Count; i++)
        {
        String s = slBrut[i];
        String[] s_tokens = s.Split(' ');
        Rights = (s_tokens.Length >= 1) ? s_tokens[0] : "";
        //StrToK( ' ', s); //hardlinks #
        sOwner = (s_tokens.Length >= 3) ? s_tokens[2] : "";
        Group  = (s_tokens.Length >= 4) ? s_tokens[3] : "";

        Rights = Rights.Remove(0, 1);//on enlève le type de fichier : directory, link, fichier...

        Calcule_Info();
        int Info_Index = slConsolidation.IndexOf(Info);
        if (-1 == Info_Index) slConsolidation.Add(Info);
        }
      Calcule_Succes();
      Add_Line((Succes ? "Succés" : "Echec ") + " "+Libelle);
      base.LED_Color = Succes ? "Lime" : "Red";
      if (!Succes)
        Display_Resultat();
      Affiche_Resultat();
      }

    // Consolidation
    protected String Rights;
    protected String sOwner;
    protected String Group;
    protected String Info;
    protected virtual void Calcule_Info()
      {
      Info = Rights + " " + sOwner + " " + Group;
      }

    //Succes
    public Boolean Succes;
    public virtual void Calcule_Succes()
      {
      Succes= true;
      }

    //Résultat
    public TStringList slConsolidation;
    public virtual void Affiche_Resultat()
      {
      LED_Color= "Lime";
      Add_Line( Libelle+':');
      slConsolidation.Sort();
      Add_Line( slConsolidation.Text());
      Add_Line( "fin consolidation droits ll:");
      //Add_Line( 'Retour commande brut:');
      //Add_Line( _Resultat);
      //Add_Line( 'Fin Retour commande brut');
      }
    }

// TVerifie_CHMOD_777

class TVerifie_CHMOD_777 : TTraite_ll
  {
  public TVerifie_CHMOD_777(TslCommande _sl) : base(_sl) { }

  //Initialisation
  public new TVerifie_CHMOD_777 Init( SshClient _ssh, TextBox _tb, StackPanel _sp, String _Repertoire)
    {
    base.Init( _ssh, _tb, _sp, _Repertoire);
    Libelle= " vérification droits 777 sur "+Repertoire;
    return this;
    }

  // Consolidation
  protected override void Calcule_Info()
    {
    Info= Rights;
    }

  //Résultat
  public override void Affiche_Resultat()
    {
    }

    //Succes
    public override void Calcule_Succes()
    {
    ExitStatus = -1;
    Error = "\n" + Error + "\n" + slConsolidation.Text()+ "\n";
    Succes= slConsolidation.Count == 1;
    if (!Succes) return;

    Succes= slConsolidation[0] == "rwxrwxrwx";//777
    if (Succes)
      ExitStatus = 0;
    }
  }

 // TVerifie_Owner_Group

 class TVerifie_Owner_Group: TTraite_ll
  {
  public TVerifie_Owner_Group(TslCommande _sl) : base(_sl) { }

  //Attributs
  public String OwnerConstraint;
  public String GroupConstraint;
  //Initialisation
  public TVerifie_Owner_Group Init( SshClient _ssh, TextBox _tb, StackPanel _sp,
                                    String _Repertoire     ,
                                    String _OwnerConstraint,
                                    String _GroupConstraint)
    {
    base.Init( _ssh, _tb, _sp, _Repertoire);
    OwnerConstraint= _OwnerConstraint;
    GroupConstraint= _GroupConstraint;
    Libelle= "vérification propriétaire "+OwnerConstraint + " groupe " + GroupConstraint+" sur "+Repertoire;
    return this;
    }
  // Consolidation
  protected override void Calcule_Info()
    {
    Info= sOwner + " " + Group;
    }
  //Résultat
  public override void Affiche_Resultat()
    {
    }
  //Succes
  public override void Calcule_Succes()
    {
    ExitStatus = -1;
    Error = "\n" + Error + "\n"+slConsolidation.Text()+ "\n";
    Succes = slConsolidation.Count == 1;
    if (!Succes) return;

    Succes= slConsolidation[0] == OwnerConstraint + " " + GroupConstraint;
    if (Succes)
      ExitStatus = 0;
      }
    }

  public class TInstallation_Check
    {
    private SshClient ssh;
    private TextBox tb;
    private StackPanel sp;
    private TslCommande sl;
    public TInstallation_Check(TextBox _tb, StackPanel _sp)
        {
        tb = _tb;
        sp= _sp;
        ssh = null;
        sl = new TslCommande();
        }
    public void Connect(String _Host, String _UserName, String _Password)
      {
      ssh = new SshClient( _Host, _UserName, _Password);
      ssh.Connect();
      }
    //Résultat brut d'une commande ssh
    public void Traite_Commande_ssh(String _Commande, String _Libelle)
        {
        TblCommande_ssh bl= new TblCommande_ssh(sl);
        bl.Init( ssh, tb, sp, _Commande, _Libelle);
        bl.Execute();
        bl.Add_Line(_Libelle);
        bl.Display_Resultat();
        }

    //Résultat brut d'une commande locale
    public void Traite_Commande_locale(String _Commande, String _Arguments, String _Libelle)
      {
      TblCommande_locale bl = new TblCommande_locale(sl);
      bl.Init( tb, sp, _Commande, _Arguments, _Libelle);
      bl.Execute();
      bl.Add_Line(_Libelle);
      bl.Display_Resultat();
      }

    //Lecture d'une valeur dans un fichier ini
    public void Valeur_dans_Fichier_ini(String _Libelle, String _IniFile, String _Section, String _Key, String _Value)
      {
      TblCommande_Inifile bl = new TblCommande_Inifile(sl);
      bl.Init(tb, sp, _IniFile, _Section, _Key, _Value, _Libelle);
      bl.Execute();
      bl.Add_Line(_Libelle);
      bl.Display_Resultat();
      }
    
    public void Traite_ll( String _Repertoire)
      {
      (new TTraite_ll(sl)).Init( ssh, tb, sp, _Repertoire).Execute();
      }

    public void Verifie_CHMOD_777( String _Repertoire)
      {
      (new TVerifie_CHMOD_777(sl)).Init( ssh, tb, sp, _Repertoire).Execute();
      }

    public void Verifie_Owner_Group( String _Repertoire, String _OwnerConstraint, String _GroupConstraint)
      {
      (new TVerifie_Owner_Group(sl)).Init( ssh, tb, sp, _Repertoire, _OwnerConstraint, _GroupConstraint).Execute();
      }
    }
  }
