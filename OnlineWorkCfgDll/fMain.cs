using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Globalization;
using SajetClass;

namespace OnlineWorkCfgDll
{
    public partial class fMain : Form
    {
        public fMain()
        {            
            InitializeComponent();
        }
       
        string g_sUserID;        
        string g_sExeName,g_sProgram,g_sFunction;
        string g_sIniFactoryID = "0";
        string g_sFCID = "0";
        string g_sTerminalID;
        string sSQL;
        DataSet dsTemp;
        string g_sIniFile = Application.StartupPath + "\\sajet.ini";        
        
        public void check_privilege()
        {
            int iPrivilege = ClientUtils.GetPrivilege(g_sUserID, g_sFunction, g_sProgram);     
            btnSave.Enabled = (iPrivilege >= 1);
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            panel1.BackgroundImage = ClientUtils.LoadImage("ImgMain.jpg");
            panel1.BackgroundImageLayout = ImageLayout.Stretch;
            panel3.BackgroundImage = ClientUtils.LoadImage("ImgFilter.jpg");
            panel3.BackgroundImageLayout = ImageLayout.Stretch;
            panel2.BackgroundImage = ClientUtils.LoadImage("ImgButton.jpg");
            panel2.BackgroundImageLayout = ImageLayout.Stretch;

            g_sUserID = ClientUtils.UserPara1;
            g_sExeName = ClientUtils.fCurrentProject;
            g_sFunction = ClientUtils.fFunctionName;
            g_sProgram = ClientUtils.fProgramName;

            //==
            this.Text = this.Text + " (" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString() + ")";
            SajetCommon.SetLanguageControl(this);
           
            check_privilege();

            //Read Ini File              
            SajetInifile sajetInifile1 = new SajetInifile();
            g_sIniFactoryID = sajetInifile1.ReadIniFile(g_sIniFile, "System", "Factory", "0");
            g_sTerminalID = sajetInifile1.ReadIniFile(g_sIniFile, g_sProgram, "Terminal", "0");            
          

            //Factory
            sSQL = "Select FACTORY_ID,FACTORY_CODE,FACTORY_NAME "
                 + "From SAJET.SYS_FACTORY "
                 + "Where ENABLED = 'Y' "
                 + "Order By Factory_COde ";
            dsTemp = ClientUtils.ExecuteSQL(sSQL);
            string sFind = "";
            for (int i = 0; i <= dsTemp.Tables[0].Rows.Count - 1; i++)
            {
                combFactory.Items.Add(dsTemp.Tables[0].Rows[i]["FACTORY_CODE"].ToString());
                if (g_sIniFactoryID == dsTemp.Tables[0].Rows[i]["FACTORY_ID"].ToString())
                {
                    sFind = dsTemp.Tables[0].Rows[i]["FACTORY_CODE"].ToString();
                }
            }

            if (sFind != "")
            {
                combFactory.SelectedIndex = combFactory.FindString(sFind);
            }
            else
            {
                combFactory.SelectedIndex = 0;
            }           
        }

        private void combFactory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LabLine.Text = "";
            LabStage.Text = "";
            LabProcess.Text = "";
            LabTerminal.Text = "";
            g_sFCID = "0";
            LabFactoryName.Text = "";

            sSQL = "Select FACTORY_ID,FACTORY_NAME "
                 + "From SAJET.SYS_FACTORY "
                 + "Where FACTORY_CODE = '" + combFactory.Text + "' ";
            dsTemp = ClientUtils.ExecuteSQL(sSQL);
            if (dsTemp.Tables[0].Rows.Count > 0)
            {
                g_sFCID = dsTemp.Tables[0].Rows[0]["FACTORY_ID"].ToString();
                LabFactoryName.Text = dsTemp.Tables[0].Rows[0]["FACTORY_NAME"].ToString();
            }

          //  Show_Terminal("Assembly");
            Show_Terminal("Repair");
        }

        public void Show_Terminal(string sProcessType)
        {
            TVTerminal.Nodes.Clear();

            sSQL = "SELECT b.pdline_name, c.stage_code, c.stage_name, d.process_code, d.process_name "
                + "       ,a.terminal_id, a.terminal_name "
                + " FROM sajet.sys_terminal a "
                + "     ,sajet.sys_pdline b "
                + "     ,sajet.sys_stage c "
                + "     ,sajet.sys_process d "
                + "     ,sajet.sys_operate_type e "
                + " WHERE b.factory_id = '" + g_sFCID + "' "
                + " AND a.pdline_id = b.pdline_id "
                + " AND a.stage_id = c.stage_id "
                + " AND a.process_id = d.process_id "
                + " AND d.operate_id = e.operate_id "
                //+ " AND Upper(e.type_name) = '" + sProcessType.ToUpper() + "' "
                + " AND a.PROCESS_id in ('1000229') "
                + " AND a.enabled = 'Y' "
                + " AND b.enabled = 'Y' "
                + " AND c.enabled = 'Y' "
                + " AND d.enabled = 'Y' "
                + " ORDER BY b.pdline_name, c.stage_code, d.process_code, a.terminal_name ";
            dsTemp = ClientUtils.ExecuteSQL(sSQL);
            if (dsTemp.Tables[0].Rows.Count == 0)
                return;

            string sPreLine = "";
            string sPreStage = "";
            string sPreProcess = "";

            for (int i = 0; i <= dsTemp.Tables[0].Rows.Count - 1; i++)
            {
                string sLine = dsTemp.Tables[0].Rows[i]["PDLINE_NAME"].ToString();
                string sStage = dsTemp.Tables[0].Rows[i]["STAGE_NAME"].ToString();
                string sProcess = dsTemp.Tables[0].Rows[i]["PROCESS_NAME"].ToString();
                string sTerminal = dsTemp.Tables[0].Rows[i]["TERMINAL_NAME"].ToString();
                
                if (sPreLine != sLine)
                {                    
                    TVTerminal.Nodes.Add(sLine);
                    int iNodeCount = TVTerminal.Nodes.Count - 1;
                    TVTerminal.Nodes[iNodeCount].ImageIndex = 0;
                    
                    TVTerminal.Nodes[iNodeCount].Nodes.Add(sStage);
                    TVTerminal.Nodes[iNodeCount].LastNode.ImageIndex = 1;

                    TVTerminal.Nodes[iNodeCount].LastNode.Nodes.Add(sProcess);
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.ImageIndex = 2;

                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.Nodes.Add(sTerminal);
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.LastNode.ImageIndex = 3;
                }
                else if (sPreStage != sStage)
                {
                    int iNodeCount = TVTerminal.Nodes.Count - 1;
                    TVTerminal.Nodes[iNodeCount].Nodes.Add(sStage);
                    TVTerminal.Nodes[iNodeCount].LastNode.ImageIndex = 1;

                    TVTerminal.Nodes[iNodeCount].LastNode.Nodes.Add(sProcess);
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.ImageIndex = 2;

                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.Nodes.Add(sTerminal);
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.LastNode.ImageIndex = 3;
                }
                else if (sPreProcess != sProcess)
                {
                    int iNodeCount = TVTerminal.Nodes.Count - 1;
                    TVTerminal.Nodes[iNodeCount].LastNode.Nodes.Add(sProcess);
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.ImageIndex = 2;

                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.Nodes.Add(sTerminal);
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.LastNode.ImageIndex = 3;
                }
                else
                {                    
                    int iNodeCount = TVTerminal.Nodes.Count - 1;
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.Nodes.Add(sTerminal);
                    TVTerminal.Nodes[iNodeCount].LastNode.LastNode.LastNode.ImageIndex = 3;
                }
                sPreLine = dsTemp.Tables[0].Rows[i]["PDLINE_NAME"].ToString();
                sPreStage = dsTemp.Tables[0].Rows[i]["STAGE_NAME"].ToString();
                sPreProcess = dsTemp.Tables[0].Rows[i]["PROCESS_NAME"].ToString();

                //SajetIniい砞﹚Terminal
                if (g_sTerminalID == dsTemp.Tables[0].Rows[i]["TERMINAL_ID"].ToString())
                {
                    TVTerminal.SelectedNode = TVTerminal.Nodes[TVTerminal.Nodes.Count - 1].LastNode.LastNode.LastNode;
                    TVTerminal.Focus();                    
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (LabTerminal.Text == "")
            {
                MessageBox.Show("Please Choose Terminal","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            string sTerminalID = Get_TerminalID();
            if (sTerminalID == "0")
            {
                return;
            }
            SajetInifile sajetInifile1 = new SajetInifile();
            sajetInifile1.WriteIniFile(g_sIniFile, g_sProgram, "Terminal", sTerminalID);
            sajetInifile1.WriteIniFile(g_sIniFile, "System", "Factory", g_sFCID);
          
            g_sTerminalID = sTerminalID;
            Close_MainForm();
            this.Close();
        }

        public string Get_TerminalID()
        {
            sSQL = "Select a.Terminal_ID "
                 + "from sajet.sys_terminal a "
                 + "    ,sajet.sys_process b "
                 + "    ,sajet.sys_pdline c "
                 + "where a.terminal_name = '" + LabTerminal.Text + "' "
                 + "and b.process_name = '" + LabProcess.Text + "' "
                 + "and c.pdline_name = '" + LabLine.Text + "' "
                 + "and a.process_id = b.process_id "
                 + "and a.pdline_id = c.pdline_id ";
            dsTemp = ClientUtils.ExecuteSQL(sSQL);
            if (dsTemp.Tables[0].Rows.Count == 0)
            {
                MessageBox.Show("Terminal Data Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "0";
            }
            return dsTemp.Tables[0].Rows[0]["TERMINAL_ID"].ToString();

        }

        private void TVTerminal_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TVTerminal.SelectedNode.SelectedImageIndex = TVTerminal.SelectedNode.ImageIndex;

            LabLine.Text = "";
            LabStage.Text = "";
            LabProcess.Text = "";
            LabTerminal.Text = "";

            if (TVTerminal.SelectedNode.Level != 3)
                return;

            LabLine.Text = TVTerminal.SelectedNode.Parent.Parent.Parent.Text;
            LabStage.Text = TVTerminal.SelectedNode.Parent.Parent.Text;
            LabProcess.Text = TVTerminal.SelectedNode.Parent.Text;
            LabTerminal.Text = TVTerminal.SelectedNode.Text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Close_MainForm()
        {
            if (this.MdiParent == null) //先判断目前是谁当家!
            {
                ClientUtils.NewSajetMES_Extensions.CloseFormByNameSpace("HWReadSnBinding"); //新版的语法只需要交给新系统处理即可
            }
            else
            {
                //将执行的主画面关掉来重新读取设定值
                foreach (Form frm in this.MdiParent.MdiChildren)
                {
                    Type t = frm.GetType();
                    if (t.Namespace == "HWReadSnBinding")
                    {
                        frm.Close();//关闭form
                    }
                }
            }
            this.Close();
        }        
    }
}

