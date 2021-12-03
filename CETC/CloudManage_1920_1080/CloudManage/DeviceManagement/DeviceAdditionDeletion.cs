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

namespace CloudManage.DeviceManagement
{
    public partial class DeviceAdditionDeletion : DevExpress.XtraEditors.XtraUserControl
    {
        private int[] selectRowDtDeviceCanDeleteEachLine = { 0 };   //当表变化时当前选中行会自动变成第一行，selectRow[0]记录的选中行重置当前选中行
        private int[] selectRowDtDeviceCanAddEachLine = { 0 };
        private CommonControl.ConfirmationBox confirmationBox1;



        public DeviceAdditionDeletion()
        {
            InitializeComponent();

            initDeviceAdditionDeletion();
        }

        void initDeviceAdditionDeletion()
        {
            initSideTileBarWorkState();
            Global.initDtDeviceCanDeleteEachLine();     //初始化可删除设备表
            refreshDtDeviceCanDeleteEachLine(this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem);
            this.gridControl_deviceAdditionDeletion.DataSource = Global.dtDeviceCanDeleteEachLine;
            if (((DataTable)this.gridControl_deviceAdditionDeletion.DataSource).Rows.Count > 0)
            {
                this.tileView1.FocusedRowHandle = selectRowDtDeviceCanDeleteEachLine[0]; //默认选中第一行
            }

            Global.initDtDeviceCanAddEachLine();   //初始化可添加设备表
        }

        private void initSideTileBarWorkState()
        {
            this.sideTileBarControl_deviceAdditionDeletion.dtInitSideTileBar = Global.dtSideTileBar;
            this.sideTileBarControl_deviceAdditionDeletion.colTagDT = "LineNO";
            this.sideTileBarControl_deviceAdditionDeletion.colTextDT = "LineName";
            this.sideTileBarControl_deviceAdditionDeletion.colNumDT = "DeviceTotalNum";
            this.sideTileBarControl_deviceAdditionDeletion._initSideTileBar();
        }

        //刷新导航目录
        void _refreshLabelDir()
        {
            string str1 = Global._getProductionLineNameByTag(this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem);
            this.labelControl_dir.Text = "   " + str1;
        }

        //记录选中行
        private void gridControl_deviceAdditionDeletion_Click(object sender, EventArgs e)
        {
            //记录选中的行
            if (((DataTable)this.gridControl_deviceAdditionDeletion.DataSource).Rows.Count > 0)   //防止查询出来的结果为空表，出现越界
            {
                selectRowDtDeviceCanDeleteEachLine = this.tileView1.GetSelectedRows();
            }
            selectRowDtDeviceCanAddEachLine[0] = this.deviceAdditionDeletion_addDeviceBox1.currentFocusRowHandler;
        }

        //填充dtDeviceCanDeleteEachLine
        void refreshDtDeviceCanDeleteEachLine(string LineNO)
        {
            /**
             *  1.  从MySQL中查出的表放到datatable中，datatable若没用Columns.Add()添加表头，query时会自动添加表头；
             *  2.  datatable初始化表头时不一定要和MySQL保持完全一致，可以用Columns.Add()手动添加MySQL表中没有的列，这样通过query填充datatable时，MySQL表中没有的列不会填充，只填充有的列。
             *  3.  添加MySQL表中没有的列，虽然冗余，但有时会简化查询过程，如Global.initDtDeviceCanDeleteEachLine()
             *  4.  使用视图存储一些小的、常用的表可以简化查询
             */
            Global.dtDeviceCanDeleteEachLine.Rows.Clear();

            MySQL.MySQLHelper mysqlHelper1 = new MySQL.MySQLHelper("localhost", "cloud_manage", "root", "ei41");
            mysqlHelper1._connectMySQL();
            //获得设备数
            int deviceCountEachLine = 0;
            DataTable dtDeviceCountEachLine = new DataTable();
            string cmdGetDeviceEnableCount = "SELECT DeviceCount FROM v_device_count_eachline WHERE LineNO='" + LineNO + "';";
            mysqlHelper1._queryTableMySQL(cmdGetDeviceEnableCount, ref dtDeviceCountEachLine);
            if (dtDeviceCountEachLine.Rows.Count == 1)
            {
                deviceCountEachLine = Convert.ToInt32(dtDeviceCountEachLine.Rows[0][0]);
            }

            for (int i = 0; i < deviceCountEachLine; i++)
            {
                DataRow dr = Global.dtDeviceCanDeleteEachLine.NewRow();
                dr["NO"] = i;
                dr["LineNO"] = LineNO;
                dr["LineName"] = Global._getProductionLineNameByTag(LineNO);
                Global.dtDeviceCanDeleteEachLine.Rows.Add(dr);
            }

            //填充DeviceNO、DeviceName、ValidParaCount
            DataTable dtDeviceNOAndValidParaCount = new DataTable();
            string cmdGetDeviceNOAndValidParaCount = "SELECT DeviceNO, ValidParaCount FROM device_info WHERE LineNO='" + LineNO + "';";
            mysqlHelper1._queryTableMySQL(cmdGetDeviceNOAndValidParaCount, ref dtDeviceNOAndValidParaCount);

            for (int i = 0; i < Global.dtDeviceCanDeleteEachLine.Rows.Count; i++)
            {
                Global.dtDeviceCanDeleteEachLine.Rows[i]["DeviceNO"] = dtDeviceNOAndValidParaCount.Rows[i]["DeviceNO"];
                Global.dtDeviceCanDeleteEachLine.Rows[i]["ValidParaCount"] = dtDeviceNOAndValidParaCount.Rows[i]["ValidParaCount"];
                Global.dtDeviceCanDeleteEachLine.Rows[i]["DeviceName"] = Global._getTestingDeviceNameByTag(dtDeviceNOAndValidParaCount.Rows[i]["DeviceNO"].ToString());
            }

            //填充DeviceFaultsCount、DeviceFaultsEnableCount
            DataTable dtDeviceFaultsCountAndFaultsEnableCount = new DataTable();
            string cmdGetDtDeviceFaultsCountAndFaultsEnableCount = "SELECT * FROM v_deviceFaultsCount_and_faultsEnableCount WHERE LineNO='" + LineNO + "';";
            mysqlHelper1._queryTableMySQL(cmdGetDtDeviceFaultsCountAndFaultsEnableCount, ref dtDeviceFaultsCountAndFaultsEnableCount);
            for (int i = 0; i < Global.dtDeviceCanDeleteEachLine.Rows.Count; i++)
            {
                string dn = Global.dtDeviceCanDeleteEachLine.Rows[i]["DeviceNO"].ToString();
                DataRow[] drs = dtDeviceFaultsCountAndFaultsEnableCount.Select("DeviceNO=" + "'" + dn + "'");
                if (drs.Length == 1)
                {
                    Global.dtDeviceCanDeleteEachLine.Rows[i]["DeviceFaultsCount"] = drs[0]["DeviceFaultsCount"];
                    Global.dtDeviceCanDeleteEachLine.Rows[i]["DeviceFaultsEnableCount"] = drs[0]["DeviceFaultsEnableCount"];
                }
            }
            mysqlHelper1.conn.Close();

            //当表中数据发生改变时，会自动选中第一行，需要用selectRow重置选中行
            if (this.gridControl_deviceAdditionDeletion.DataSource != null)
            {
                this.tileView1.FocusedRowHandle = selectRowDtDeviceCanDeleteEachLine[0];
            }
        }

        private void refreshDtDeviceCanAddEachLine(string LineNO)
        {
            Global.dtDeviceCanAddEachLine.Rows.Clear();
            //显示每台产线可添加设备的grid绑定表填充数据
            DataRow[] drDeviceConfigSelected = Global.dtDeviceConfig.Select("LineNO='" + LineNO + "'");
            if (drDeviceConfigSelected.Length == 1)
            {
                string[] colNames = Global.GetColumnsByDataTable(Global.dtDeviceConfig);
                for (int i = 2; i < Global.dtDeviceConfig.Columns.Count; i++)
                {
                    if (drDeviceConfigSelected[0][i].ToString() == "0")
                    {
                        //填充DeviceNO
                        string deviceNO = String.Empty;
                        for (int j = 0; j < colNames[i].Length; j++)
                        {
                            if (colNames[i].ElementAt(j) >= '0' && colNames[i].ElementAt(j) <= '9')
                            {
                                deviceNO += colNames[i].ElementAt(j);
                            }
                        }

                        //填充DeviceName
                        string deviceName = String.Empty;
                        DataRow[] drDeviceName = Global.dtTestingDeviceName.Select("DeviceNO='" + deviceNO + "'");
                        if (drDeviceName.Length == 1)
                        {
                            deviceName = drDeviceName[0]["DeviceName"].ToString();
                        }

                        DataRow dr = Global.dtDeviceCanAddEachLine.NewRow();
                        dr["DeviceNO"] = deviceNO;
                        dr["DeviceName"] = deviceName;
                        Global.dtDeviceCanAddEachLine.Rows.Add(dr);
                    }
                }
            }
            //填充NO
            Global.reorderDt(ref Global.dtDeviceCanAddEachLine);

            //当表中数据发生改变时，会自动选中第一行，需要用selectRow重置选中行
            if (this.deviceAdditionDeletion_addDeviceBox1 != null)
            {
                this.deviceAdditionDeletion_addDeviceBox1.currentFocusRowHandler = selectRowDtDeviceCanAddEachLine[0];
            }
        }

        private void sideTileBarControl_deviceAdditionDeletion_sideTileBarItemSelectedChanged(object sender, EventArgs e)
        {
            _refreshLabelDir();
            refreshDtDeviceCanDeleteEachLine(this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem);   //刷新可删除列表
            refreshDtDeviceCanAddEachLine(this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem);      //刷新可添加列表
        }

        

        private void simpleButton_deviceAddition_Click(object sender, EventArgs e)
        {
            //创建设备添加框
            this.deviceAdditionDeletion_addDeviceBox1 = new DeviceAdditionDeletion_addDeviceBox();
            this.deviceAdditionDeletion_addDeviceBox1.Location = new System.Drawing.Point(535, 252);
            this.deviceAdditionDeletion_addDeviceBox1.Name = "deviceAdditionDeletion_addDeviceBox1";
            this.deviceAdditionDeletion_addDeviceBox1.Size = new System.Drawing.Size(550, 548);
            this.deviceAdditionDeletion_addDeviceBox1.TabIndex = 29;
            this.deviceAdditionDeletion_addDeviceBox1.titleAddDeviceBox = "添加设备";
            this.deviceAdditionDeletion_addDeviceBox1.AddDeviceBoxOKClicked += new DeviceAdditionDeletion_addDeviceBox.SimpleButtonOKClickHanlder(this.deviceAdditionDeletion_addDeviceBox1_AddDeviceBoxOKClicked);
            this.deviceAdditionDeletion_addDeviceBox1.AddDeviceBoxCancelClicked += new DeviceAdditionDeletion_addDeviceBox.SimpleButtonOKClickHanlder(this.deviceAdditionDeletion_addDeviceBox1_AddDeviceBoxCancelClicked);
            this.Controls.Add(this.deviceAdditionDeletion_addDeviceBox1);
            this.deviceAdditionDeletion_addDeviceBox1.Visible = true;
            this.deviceAdditionDeletion_addDeviceBox1.BringToFront();
            this.deviceAdditionDeletion_addDeviceBox1.dataSource = Global.dtDeviceCanAddEachLine;

            refreshDtDeviceCanAddEachLine(this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem);
        }

        private void simpleButton_deviceDeletion_Click(object sender, EventArgs e)
        {
            //弹出确认框
            this.confirmationBox1 = new CommonControl.ConfirmationBox();
            this.confirmationBox1.Appearance.BackColor = System.Drawing.Color.White;
            this.confirmationBox1.Appearance.Options.UseBackColor = true;
            this.confirmationBox1.Location = new System.Drawing.Point(635, 300);
            this.confirmationBox1.Name = "confirmationBox1";
            this.confirmationBox1.Size = new System.Drawing.Size(350, 200);
            this.confirmationBox1.TabIndex = 29;
            this.confirmationBox1.titleConfirmationBox = "确认删除 " + Global._getTestingDeviceNameByTag(this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem) + "?";
            this.confirmationBox1.ConfirmationBoxOKClicked += new CommonControl.ConfirmationBox.SimpleButtonOKClickHanlder(this.confirmationBox1_ConfirmationBoxOKClicked);
            this.confirmationBox1.ConfirmationBoxCancelClicked += new CommonControl.ConfirmationBox.SimpleButtonCancelClickHanlder(this.confirmationBox1_ConfirmationBoxCancelClicked);
            this.Controls.Add(this.confirmationBox1);
            this.confirmationBox1.Visible = true;
            this.confirmationBox1.BringToFront();

        }

        private void confirmationBox1_ConfirmationBoxOKClicked(object sender, EventArgs e)
        {
            MySQL.MySQLHelper mysqlHelper1 = new MySQL.MySQLHelper("localhost", "cloud_manage", "root", "ei41");
            mysqlHelper1._connectMySQL();

            DataRow drSelected = tileView1.GetDataRow(selectRowDtDeviceCanDeleteEachLine[0]);    //获取的是grid绑定的表所有列，而不仅仅是显示出来的列

            MySqlParameter lineNO = new MySqlParameter("ln", MySqlDbType.VarChar, 20);
            lineNO.Value = this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem;
            MySqlParameter deviceNO = new MySqlParameter("dn", MySqlDbType.VarChar, 20);
            deviceNO.Value = drSelected["DeviceNO"];
            MySqlParameter ifAffected = new MySqlParameter("ifRowAffected", MySqlDbType.Int32, 1);
            MySqlParameter[] paras = { lineNO, deviceNO, ifAffected };
            string cmdDeleteDevice = "p_deleteDevice";
            mysqlHelper1._executeProcMySQL(cmdDeleteDevice, paras, 2, 1);

            //string cmdDeleteDevice = "CALL p_deleteDevice('" + this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem + "', '" + drSelected["DeviceNO"] + "');";
            //mysqlHelper1._updateMySQL(cmdDeleteDevice);

            this.confirmationBox1.Visible = false;
            refreshDtDeviceCanDeleteEachLine(this.sideTileBarControl_deviceAdditionDeletion.tagSelectedItem);   //刷新grid显示

            if (Convert.ToInt32(ifAffected.Value) == 1)
            {
                MessageBox.Show("删除成功");
            }
            else if (Convert.ToInt32(ifAffected.Value) == 0)
            {
                MessageBox.Show("删除失败");
            }
            mysqlHelper1.conn.Close();
        }

        private void confirmationBox1_ConfirmationBoxCancelClicked(object sender, EventArgs e)
        {
            this.confirmationBox1.Visible = false;
        }

        private void deviceAdditionDeletion_addDeviceBox1_AddDeviceBoxOKClicked(object sender, EventArgs e)
        {
            

        }

        private void deviceAdditionDeletion_addDeviceBox1_AddDeviceBoxCancelClicked(object sender, EventArgs e)
        {
            this.deviceAdditionDeletion_addDeviceBox1.Visible = false;
        }



    }
}