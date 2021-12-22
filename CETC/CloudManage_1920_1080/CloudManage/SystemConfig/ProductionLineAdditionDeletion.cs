﻿using DevExpress.XtraEditors;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudManage.SystemConfig
{
    public partial class ProductionLineAdditionDeletion : DevExpress.XtraEditors.XtraUserControl
    {
        private int[] selectRowDtProductionLineExists = { 0 };
        private CommonControl.ConfirmationBox confirmationBox_delLine;
        private CommonControl.ConfirmationBox confirmationBox_inputLineName;
        private CommonControl.ConfirmationBox confirmationBox_addLine;
        private CommonControl.ConfirmationBox confirmationBox_modifyLineName;
        private CommonControl.ConfirmationBox confirmationBox_copyLine;


        string inputLineName = String.Empty;
        string[] lineNOVec = new string[999];   //暂存所有可能的LineNO

        public ProductionLineAdditionDeletion()
        {
            InitializeComponent();
            initDeviceAdditionDeletion();
        }

        void initDeviceAdditionDeletion()
        {
            initLineNOVec();
            Global.initDtProductionLineExists();
            initSideTileBarProductionLineAdditionDeletion();
            this.gridControl_productionLineAdditionDeletion.DataSource = Global.dtProductionLineSystemConfig;
            dtucTextBoxEx1.HandInputExePath = "HandInput\\handinput.exe";

            if (((DataTable)this.gridControl_productionLineAdditionDeletion.DataSource).Rows.Count > 0)
            {
                this.tileView1.FocusedRowHandle = selectRowDtProductionLineExists[0]; //默认选中第一行
            }
        }

        private void initLineNOVec()
        {
            for (int i = 1; i <= 9; i++)
            {
                lineNOVec[i - 1] = "00" + i.ToString();
            }
            for (int i = 10; i <= 99; i++)
            {
                lineNOVec[i - 1] = "0" + i.ToString();
            }
            for (int i = 100; i <= 999; i++)
            {
                lineNOVec[i - 1] = i.ToString();
            }
        }

        //生成一个在表中未用的LineNO
        private string createLineNO()
        {
            string lNO = String.Empty;
            List<string> ll = new List<string>();
            for (int i = 0; i < Global.dtProductionLine.Rows.Count; i++)
            {
                ll.Add(Global.dtProductionLine.Rows[i]["LineNO"].ToString());
            }

            for (int i = 0; i < 999; i++)
            {
                if (ll.Contains(lineNOVec[i]) == false)
                {
                    lNO = lineNOVec[i];
                    break;
                }
            }
            return lNO;
        }

        private void initSideTileBarProductionLineAdditionDeletion()
        {
            this.sideTileBarControl_productionLineAdditionDeletion.overViewText = "产线";
            string totalProductionCount = Global.dtSideTileBar.Rows.Count.ToString();
            this.sideTileBarControl_productionLineAdditionDeletion._setNum("000", totalProductionCount);
        }

        private void refreshDtProductionLineSystemConfig()
        {
            Global._init_dtSideTileBarWorkState();
            Global.initDtProductionLineExists();
        }

        //记录选中的行
        private void gridControl_productionLineAdditionDeletion_Click(object sender, EventArgs e)
        {
            if (((DataTable)this.gridControl_productionLineAdditionDeletion.DataSource).Rows.Count > 0)   //防止查询出来的结果为空表，出现越界
            {
                selectRowDtProductionLineExists = this.tileView1.GetSelectedRows();
            }
        }

        /*********************************************删除产线*******************************************************/
        private void simpleButton_productionLineDeletion_Click(object sender, EventArgs e)
        {
            if (Global.dtProductionLineSystemConfig.Rows.Count != 0)
            {
                //弹出确认框
                this.confirmationBox_delLine = new CommonControl.ConfirmationBox();
                this.confirmationBox_delLine.Appearance.BackColor = System.Drawing.Color.White;
                this.confirmationBox_delLine.Appearance.Options.UseBackColor = true;
                this.confirmationBox_delLine.Location = new System.Drawing.Point(624, 200);
                this.confirmationBox_delLine.Name = "confirmationBox1";
                this.confirmationBox_delLine.Size = new System.Drawing.Size(350, 200);
                this.confirmationBox_delLine.TabIndex = 29;
                DataRow drSelected = tileView1.GetDataRow(selectRowDtProductionLineExists[0]);    //获取的是grid绑定的表所有列，而不仅仅是显示出来的列
                this.confirmationBox_delLine.titleConfirmationBox = "确认删除  " + Global._getProductionLineNameByTag(drSelected["LineNO"].ToString()) + "  ?";
                this.confirmationBox_delLine.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_delLine_ConfirmationBoxOKClicked);
                this.confirmationBox_delLine.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_delLine_ConfirmationBoxCancelClicked);
                this.Controls.Add(this.confirmationBox_delLine);
                this.confirmationBox_delLine.Visible = true;
                this.confirmationBox_delLine.BringToFront();
            }
        }

        private void confirmationBox_delLine_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            if (Global.dtProductionLineSystemConfig.Rows.Count != 0)
            {
                this.confirmationBox_delLine.titleConfirmationBox = " 删除将无法恢复，确认删除？";
                this.confirmationBox_delLine.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_delLine_ConfirmationBoxOKDoubleCheckClicked);
                this.confirmationBox_delLine.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_delLine_ConfirmationBoxCancelDoubleCheckClicked);
            }
        }

        private void confirmationBox_delLine_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.confirmationBox_delLine.Visible = false;
        }

        private void confirmationBox_delLine_ConfirmationBoxOKDoubleCheckClicked(object sender, EventArgs e)
        {
            if (Global.dtProductionLineSystemConfig.Rows.Count != 0)
            {
                DataRow drSelected = this.tileView1.GetDataRow(selectRowDtProductionLineExists[0]);

                MySqlParameter lineNO = new MySqlParameter("ln", MySqlDbType.VarChar, 20);
                lineNO.Value = drSelected["LineNO"].ToString();
                MySqlParameter ifAffectedDelLine = new MySqlParameter("ifAffectedRowDelLine_", MySqlDbType.Int32, 1);
                MySqlParameter[] paras = { lineNO, ifAffectedDelLine };
                string cmdDeleteLine = "p_deleteLine";
                Global.mysqlHelper1._executeProcMySQL(cmdDeleteLine, paras, 1, 1);


                this.confirmationBox_delLine.Visible = false;

                if (Convert.ToInt32(ifAffectedDelLine.Value) == 1)
                {
                    MessageBox.Show("删除成功");
                    Global.ifLineAdditionOrDeletion = true;
                    refreshDtProductionLineSystemConfig();

                    //只需重读当前页面用到的表，其他表，重启时会更新
                    //重读device_config
                    Global._init_dtDeviceConfig();
                    //重读productionLine
                    Global._init_dtProductionLine();
                    //重读device_info

                    //重读device_info_paranameandsuffix

                    //重读device_info_threshold

                    //重读faults_config

                    //重读faults_current

                    //重读faults_history
                }
                else
                {
                    MessageBox.Show("删除失败");
                }
            }
        }

        private void confirmationBox_delLine_ConfirmationBoxCancelDoubleCheckClicked(object sender, EventArgs e)
        {
            this.confirmationBox_delLine.Visible = false;
        }

        /*********************************************添加产线*******************************************************/
        private void simpleButton_productionLineAddition_Click(object sender, EventArgs e)
        {
            this.confirmationBox_inputLineName = new CommonControl.ConfirmationBox();
            this.confirmationBox_inputLineName.Appearance.BackColor = System.Drawing.Color.White;
            this.confirmationBox_inputLineName.Appearance.Options.UseBackColor = true;
            this.confirmationBox_inputLineName.Location = new System.Drawing.Point(624, 200);
            this.confirmationBox_inputLineName.Name = "confirmationBox1";
            this.confirmationBox_inputLineName.Size = new System.Drawing.Size(350, 200);
            this.confirmationBox_inputLineName.TabIndex = 29;
            this.confirmationBox_inputLineName.titleConfirmationBox = "请输入产线名称";
            this.confirmationBox_inputLineName.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_inputLineName_ConfirmationBoxOKClicked);
            this.confirmationBox_inputLineName.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_inputLineName_ConfirmationBoxCancelClicked);
            this.Controls.Add(this.confirmationBox_inputLineName);
            this.confirmationBox_inputLineName.Visible = true;
            this.confirmationBox_inputLineName.BringToFront();

            this.dtucTextBoxEx1.Visible = true;
        }

        private void confirmationBox_inputLineName_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            if (this.dtucTextBoxEx1.InputText != "")
                inputLineName = this.dtucTextBoxEx1.InputText;
            this.dtucTextBoxEx1.InputText = "";
            if (inputLineName != "")
            {
                this.dtucTextBoxEx1.Visible = false;
                this.confirmationBox_inputLineName.Visible = false;

                this.confirmationBox_addLine = new CommonControl.ConfirmationBox();
                this.confirmationBox_addLine.Appearance.BackColor = System.Drawing.Color.White;
                this.confirmationBox_addLine.Appearance.Options.UseBackColor = true;
                this.confirmationBox_addLine.Location = new System.Drawing.Point(624, 200);
                this.confirmationBox_addLine.Name = "confirmationBox1";
                this.confirmationBox_addLine.Size = new System.Drawing.Size(350, 200);
                this.confirmationBox_addLine.TabIndex = 29;
                this.confirmationBox_addLine.titleConfirmationBox = "确认添加产线   " + inputLineName + " ?";
                this.confirmationBox_addLine.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_addLine_ConfirmationBoxOKClicked);
                this.confirmationBox_addLine.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_addLine_ConfirmationBoxCancelClicked);
                this.Controls.Add(this.confirmationBox_addLine);
                this.confirmationBox_addLine.Visible = true;
                this.confirmationBox_addLine.BringToFront();
            }
        }

        private void confirmationBox_inputLineName_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.dtucTextBoxEx1.InputText = "";
            this.dtucTextBoxEx1.Visible = false;
            this.confirmationBox_inputLineName.Visible = false;
        }

        private void confirmationBox_addLine_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            MySqlParameter lineNO = new MySqlParameter("ln", MySqlDbType.VarChar, 20);
            lineNO.Value = createLineNO();
            MySqlParameter lineName = new MySqlParameter("lname", MySqlDbType.VarChar, 20);
            lineName.Value = inputLineName;
            MySqlParameter ifAffected = new MySqlParameter("ifRowAffected", MySqlDbType.Int32, 1);
            MySqlParameter[] paras = { lineNO, lineName, ifAffected };
            string cmdAddLine = "p_addLine";
            Global.mysqlHelper1._executeProcMySQL(cmdAddLine, paras, 2, 1);

            this.confirmationBox_addLine.Visible = false;

            if (Convert.ToInt32(ifAffected.Value) == 1)
            {
                MessageBox.Show("添加成功");
                Global.ifLineAdditionOrDeletion = true;
                refreshDtProductionLineSystemConfig();

                //重读device_config
                Global._init_dtDeviceConfig();
                //重读productionLine
                Global._init_dtProductionLine();
            }
            else
            {
                MessageBox.Show("添加失败");
            }
        }

        private void confirmationBox_addLine_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.confirmationBox_addLine.Visible = false;
        }

        /*********************************************修改产线名*******************************************************/
        private void simpleButton_lineNameModify_Click(object sender, EventArgs e)
        {
            if (Global.dtProductionLineSystemConfig.Rows.Count != 0 && this.selectRowDtProductionLineExists.Length != 0)
            {
                this.dtucTextBoxEx1.Visible = true;
                //弹出确认框
                this.confirmationBox_modifyLineName = new CommonControl.ConfirmationBox();
                this.confirmationBox_modifyLineName.Appearance.BackColor = System.Drawing.Color.White;
                this.confirmationBox_modifyLineName.Appearance.Options.UseBackColor = true;
                this.confirmationBox_modifyLineName.Location = new System.Drawing.Point(624, 200);
                this.confirmationBox_modifyLineName.Name = "confirmationBox1";
                this.confirmationBox_modifyLineName.Size = new System.Drawing.Size(350, 200);
                this.confirmationBox_modifyLineName.TabIndex = 29;
                DataRow drSelected = tileView1.GetDataRow(selectRowDtProductionLineExists[0]);    //获取的是grid绑定的表所有列，而不仅仅是显示出来的列
                this.confirmationBox_modifyLineName.titleConfirmationBox = "确认修改  " + Global._getProductionLineNameByTag(drSelected["LineNO"].ToString()) + "  ?";
                this.confirmationBox_modifyLineName.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_modifyLineName_ConfirmationBoxOKClicked);
                this.confirmationBox_modifyLineName.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_modifyLineName_ConfirmationBoxCancelClicked);
                this.Controls.Add(this.confirmationBox_modifyLineName);
                this.confirmationBox_modifyLineName.Visible = true;
                this.confirmationBox_modifyLineName.BringToFront();
            }
        }

        private void confirmationBox_modifyLineName_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            if (this.dtucTextBoxEx1.InputText != "")
            {
                inputLineName = this.dtucTextBoxEx1.InputText;
                this.dtucTextBoxEx1.InputText = "";
                this.dtucTextBoxEx1.Visible = false;
                this.confirmationBox_modifyLineName.titleConfirmationBox = "确认将产线名修改为   " + inputLineName + "  ?";
                this.confirmationBox_modifyLineName.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_modifyLineNameCheck_ConfirmationBoxOKClicked);
                this.confirmationBox_modifyLineName.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_modifyLineNameCheck_ConfirmationBoxCancelClicked);
                this.confirmationBox_modifyLineName.BringToFront();
            }
        }

        private void confirmationBox_modifyLineName_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.dtucTextBoxEx1.Visible = false;
            this.confirmationBox_modifyLineName.Visible = false;
        }

        private void confirmationBox_modifyLineNameCheck_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            if (Global.dtProductionLineSystemConfig.Rows.Count != 0)
            {
                this.dtucTextBoxEx1.Visible = false;

                DataRow drSelected = this.tileView1.GetDataRow(selectRowDtProductionLineExists[0]);

                MySqlParameter lineNO = new MySqlParameter("ln", MySqlDbType.VarChar, 20);
                lineNO.Value = drSelected["LineNO"].ToString();
                MySqlParameter lineName = new MySqlParameter("lName", MySqlDbType.VarChar, 20);
                lineName.Value = inputLineName;

                MySqlParameter ifAffectedModifyLineName = new MySqlParameter("ifAffectedRowModifyLine_", MySqlDbType.Int32, 1);
                MySqlParameter[] paras = { lineNO, lineName, ifAffectedModifyLineName };
                string cmdModifyLineName = "p_modifyLineName";
                Global.mysqlHelper1._executeProcMySQL(cmdModifyLineName, paras, 2, 1);

                confirmationBox_modifyLineName.Visible = false;

                if (Convert.ToInt32(ifAffectedModifyLineName.Value) == 1)
                {
                    MessageBox.Show("修改成功");
                    Global.ifLineAdditionOrDeletion = true;
                    refreshDtProductionLineSystemConfig();
                    //重读productionLine
                    Global._init_dtProductionLine();
                }
                else
                {
                    MessageBox.Show("修改失败");
                }
            }
        }

        private void confirmationBox_modifyLineNameCheck_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.dtucTextBoxEx1.Visible = false;
            this.confirmationBox_modifyLineName.Visible = false;
        }

        /*********************************************复制产线*******************************************************/
        private void simpleButton_productionLineCopy_Click(object sender, EventArgs e)
        {
            if (Global.dtProductionLineSystemConfig.Rows.Count != 0)
            {
                DataRow drSelected = this.tileView1.GetDataRow(selectRowDtProductionLineExists[0]);

                this.confirmationBox_copyLine = new CommonControl.ConfirmationBox();
                this.confirmationBox_copyLine.Appearance.BackColor = System.Drawing.Color.White;
                this.confirmationBox_copyLine.Appearance.Options.UseBackColor = true;
                this.confirmationBox_copyLine.Location = new System.Drawing.Point(624, 200);
                this.confirmationBox_copyLine.Name = "confirmationBox1";
                this.confirmationBox_copyLine.Size = new System.Drawing.Size(350, 200);
                this.confirmationBox_copyLine.TabIndex = 29;
                this.confirmationBox_copyLine.titleConfirmationBox = "确认拷贝产线   " + Global._getProductionLineNameByTag(drSelected["LineNO"].ToString()) + "  ?";
                this.confirmationBox_copyLine.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_copyLine_ConfirmationBoxOKClicked);
                this.confirmationBox_copyLine.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_copyLine_ConfirmationBoxCancelClicked);
                this.Controls.Add(this.confirmationBox_copyLine);
                this.confirmationBox_copyLine.Visible = true;
                this.confirmationBox_copyLine.BringToFront();
            }
        }

        private void confirmationBox_copyLine_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            this.dtucTextBoxEx1.Visible = true;
            this.confirmationBox_copyLine.titleConfirmationBox = "请输入产线名";
            this.confirmationBox_copyLine.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_copyLineCheck_ConfirmationBoxOKClicked);
            this.confirmationBox_copyLine.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_copyLineCheck_ConfirmationBoxCancelClicked);
            this.confirmationBox_copyLine.BringToFront();
        }

        private void confirmationBox_copyLine_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.confirmationBox_copyLine.Visible = false;
            this.dtucTextBoxEx1.Visible = false;
            inputLineName = "";
            this.dtucTextBoxEx1.InputText = "";
        }

        //读取被选中产线的LineNO在相关表中的记录。替换LineName、LineNO将记录重新插入
        private void confirmationBox_copyLineCheck_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            if (this.dtucTextBoxEx1.InputText != "")
                inputLineName = this.dtucTextBoxEx1.InputText;
            this.dtucTextBoxEx1.InputText = "";
            if (inputLineName != "")
            {
                this.confirmationBox_copyLine.titleConfirmationBox = "确认产线名为 " + inputLineName + " ?";
                this.confirmationBox_copyLine.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox_copyLineCheckLineName_ConfirmationBoxOKClicked);
                this.confirmationBox_copyLine.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox_copyLineCheckLineName_ConfirmationBoxCancelClicked);
                this.confirmationBox_copyLine.BringToFront();
            }
        }

        private void confirmationBox_copyLineCheck_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.confirmationBox_copyLine.Visible = false;
            this.dtucTextBoxEx1.Visible = false;
        }

        //将表中指定lineNO的记录复制
        private bool lineDtCopy(string tableName, ref DataTable dtWithLineNO, string srcLineNO, string dstLineNO)
        {
            string[] colNamesTable = Global.GetColumnsByDataTable(dtWithLineNO);
            string cmdDtWithLineNOCopy = "INSERT INTO " + tableName + " (LineNO";
            for (int i = 2; i < colNamesTable.Length; i++)
            {
                cmdDtWithLineNOCopy += ", " + colNamesTable[i];
            }
            cmdDtWithLineNOCopy += ") SELECT '" + dstLineNO + "' AS LineNO";
            for (int i = 2; i < colNamesTable.Length; i++)
            {
                cmdDtWithLineNOCopy += ", " + colNamesTable[i];
            }
            cmdDtWithLineNOCopy += " FROM " + tableName + " WHERE LineNO='" + srcLineNO + "';";
            return Global.mysqlHelper1._insertMySQL(cmdDtWithLineNOCopy) == true ? true : false;
        }

        private void confirmationBox_copyLineCheckLineName_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            if (inputLineName != "")
            {
                this.dtucTextBoxEx1.InputText = "";
                this.dtucTextBoxEx1.Visible = false;
                this.confirmationBox_copyLine.Visible = false;

                DataRow drSelected = this.tileView1.GetDataRow(selectRowDtProductionLineExists[0]);
                string lineNOSelected = drSelected["LineNO"].ToString();
                string lineNODst = createLineNO();
                //productionline
                string cmdProductionLineCopy = "INSERT INTO productionline (LineNO, LineName) VALUES ('" + createLineNO() + "', '" + inputLineName + "');";
                bool flagProductionLineCopy = Global.mysqlHelper1._insertMySQL(cmdProductionLineCopy);

                //device_config
                bool flagDevice_flag = lineDtCopy("device_config", ref Global.dtDeviceConfig, lineNOSelected, lineNODst);

                //device_info
                string cmdDevice_info = "SELECT * FROM device_info;";
                DataTable dtDevice_info = new DataTable();
                Global.mysqlHelper1._queryTableMySQL(cmdDevice_info, ref dtDevice_info);
                bool flagDevice_infoCopy = lineDtCopy("device_info", ref dtDevice_info, lineNOSelected, lineNODst);

                //device_info_paranameandsuffix
                string cmdDevice_info_paranameandsuffix = "SELECT * FROM device_info_paranameandsuffix;";
                DataTable dtDevice_info_paranameandsuffix = new DataTable();
                Global.mysqlHelper1._queryTableMySQL(cmdDevice_info_paranameandsuffix, ref dtDevice_info_paranameandsuffix);
                bool flagDevice_info_paranameandsuffixCopy = lineDtCopy("device_info_paranameandsuffix", ref dtDevice_info_paranameandsuffix, lineNOSelected, lineNODst);

                //device_info_threshold
                string cmdDevice_info_threshold = "SELECT * FROM device_info_threshold;";
                DataTable dtDevice_info_threshold = new DataTable();
                Global.mysqlHelper1._queryTableMySQL(cmdDevice_info_threshold, ref dtDevice_info_threshold);
                bool flagDevice_info_threshold = lineDtCopy("device_info_threshold", ref dtDevice_info_threshold, lineNOSelected, lineNODst);

                //faults_config
                string cmdFaults_config = "SELECT * FROM faults_config;";
                DataTable dtFaults_config = new DataTable();
                Global.mysqlHelper1._queryTableMySQL(cmdFaults_config, ref dtFaults_config);
                bool flagFaults_config = lineDtCopy("faults_config", ref dtFaults_config, lineNOSelected, lineNODst);

                if (flagProductionLineCopy && flagDevice_flag && flagDevice_info_paranameandsuffixCopy && flagDevice_info_threshold && flagFaults_config)
                {
                    MessageBox.Show("产线复制成功");
                    Global.ifLineAdditionOrDeletion = true;
                    refreshDtProductionLineSystemConfig();
                }
                else
                {
                    MessageBox.Show("产线复制失败，请清理已添加记录的表");
                }
            }
        }

        private void confirmationBox_copyLineCheckLineName_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.confirmationBox_copyLine.Visible = false;
            this.dtucTextBoxEx1.Visible = false;
            inputLineName = "";
        }

        private void dtucTextBoxEx1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
