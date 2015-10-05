using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EdgeJs;
using zkemkeeper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System.IO;

namespace FingerprintSync
{
    public partial class Form1 : Form
    {
        private int stat = 0;
        
        private string AppID = "";
        private AddDevice addDeviceForm;
        private int devicesCount = 0;
        #region Class Properties
        private DataTable configParam;
        private Boolean deviceMatch = false;
        
        public DataTable ConfigParam
        {
            get
            {
                return configParam;
            }
            set
            {
                configParam = value;
            }
        }

        private DataTable dtDevices;
        public DataTable DtDevices
        {
            get
            {
                return dtDevices;
            }
            set
            {
                dtDevices = value;
            }
        }

        private Client socket;
        public Client Socket
        {
            get
            {
                return socket;
            }
            set
            {
                socket = value;
            }
        }

        private DataTable dtEnroll;
        public DataTable DtEnroll
        {
            get
            {
                return dtEnroll;
            }
            set
            {
                dtEnroll = value;
            }
        }

        private System.Windows.Forms.Timer systemTimer;
        public System.Windows.Forms.Timer SystemTimer
        {
            get
            {
                return systemTimer;
            }
            set
            {
                systemTimer = value;
            }
        }

        #endregion

        private int timerCounter = 60;
        public Form1(dynamic config)
        {
            InitializeComponent();
            
            StructuringDtDevices();
            StructuringDtEnroll();

            ApplyConfigFile(config);      

            AppIDTextbox.Text = configParam.Rows[0]["AppID"].ToString();
            AppID = configParam.Rows[0]["AppID"].ToString();

            systemTimer = new System.Windows.Forms.Timer();
            systemTimer.Interval = 1000;
            systemTimer.Tick += new EventHandler(systemTimer_Tick);
            systemTimer.Start();
            connectAllDevices();
        }

        void systemTimer_Tick(object sender, EventArgs e)
        {
            timerCounter--;
            if (timerCounter == 0)
            {
                timerCounter = 60;
                CheckingDeviceConnection();
            }
        }

        private void StructuringDtEnroll()
        {  
            dtEnroll = new DataTable();
            dtEnroll.Columns.Add("serial_number");
            dtEnroll.Columns.Add("employee_number");
            dtEnroll.Columns.Add("commandID");
            dtEnroll.Columns.Add("zkclass", typeof(zkemkeeper.CZKEMClass));
            dtEnroll.Columns.Add("fdid");
            dtEnroll.Columns.Add("full_name");
        }

        private void CheckingDeviceConnection()
        {
            foreach (DataRow device in dtDevices.Rows)
            {
                if (device["status"] == "connected")
                {
                    if (device["assign_status"].ToString() == "match")
                    {
                        CZKEMClass fdevice = (CZKEMClass)device["zkclass"];
                        int idwErrorCode = 0;

                        int idwInfo = 1;//the only possible value
                        string sValue = "";

                        Cursor = Cursors.WaitCursor;
                        if (fdevice.GetDeviceStrInfo(Convert.ToInt16(device["fdid"]), idwInfo, out sValue))
                        {
                            ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " is connected with value check : " + sValue);
                        }
                        else
                        {
                            fdevice.GetLastError(ref idwErrorCode);
                            ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " is not connected with error code : " + idwErrorCode.ToString());
                            device["status"] = "not_connected";

                            fdevice.OnFinger -= new _IZKEMEvents_OnFingerEventHandler(FDevice_OnFinger);
                            fdevice.OnVerify -= new _IZKEMEvents_OnVerifyEventHandler(FDevice_OnVerify);
                            fdevice.OnEnrollFingerEx -= new _IZKEMEvents_OnEnrollFingerExEventHandler(FDevice_OnEnrollFingerEx);
                            fdevice.OnAttTransactionEx -= new _IZKEMEvents_OnAttTransactionExEventHandler(FDevice_OnAttTransactionEx);

                        }
                    }
                    else
                    {
                        ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " is not match with configuration. Contact your system administrator");
                        RTESocketIOEmit("daemon_device_checking", "Device " + device["serial_number"].ToString() + " is not match with configuration");
                    }
                }
                else
                {
                    string ip = device["ip_local"].ToString();
                    int port = Convert.ToInt16(device["port"]);
                    int commPassword = Convert.ToInt32(device["comm_password"]);
                    int fdid = Convert.ToInt16(device["fdid"]);
                    CZKEMClass FDevice = (CZKEMClass)device["zkclass"];
                    ConsoleWriteLineInThread("Connecting to device " + device["serial_number"].ToString() + " on " + ip + "...");

                    FDevice.SetCommPassword(commPassword);

                    if (FDevice.Connect_Net(ip, port))
                    {
                        ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " connected on " + ip);
                        if (FDevice.RegEvent(fdid, 65535))
                        {
                            FDevice.OnFinger += new _IZKEMEvents_OnFingerEventHandler(FDevice_OnFinger);
                            FDevice.OnVerify += new _IZKEMEvents_OnVerifyEventHandler(FDevice_OnVerify);
                            FDevice.OnEnrollFingerEx += new _IZKEMEvents_OnEnrollFingerExEventHandler(FDevice_OnEnrollFingerEx);
                            FDevice.OnAttTransactionEx += new _IZKEMEvents_OnAttTransactionExEventHandler(FDevice_OnAttTransactionEx);
                            //button3.Click += new EventHandler(test_click);
                        }
                        device["status"] = "connected";
                        UpdateCountDeviceLabel(dtDevices.Rows.Count.ToString());
                        //countDeviceLabel.Text = (Convert.ToInt16(countDeviceLabel.Text) + 1).ToString();
                        if (device["serial_number"].ToString() == GetDeviceSerialNumber(FDevice, Convert.ToInt16(device["fdid"])))
                        {
                            device["assign_status"] = "match";
                            ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " is match with configuration");
                        }
                        else
                        {
                            ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " is not match with configuration. Contact your system administrator");
                        }
                    }
                    else
                    {
                        ConsoleWriteLineInThread("Unable to connect device " + device["serial_number"].ToString() + " on " + ip);
                        device["status"] = "not_connected";
                    }
                }
            }
        }

        private void ApplyConfigFile(dynamic configJson)
        {
            configParam = new DataTable();
            configParam.Columns.Add("AppID");
            configParam.Columns.Add("Host");

            //configParam.Columns.Add("devices", typeof(System.Data.DataTable));

            DataRow rowAdd = configParam.NewRow();
            rowAdd["AppID"] = configJson.AppID;
            rowAdd["Host"] = configJson.Host;

            foreach (var device in configJson.devices)
            {
                DataRow rowDevice = dtDevices.NewRow();

                rowDevice["serial_number"] = device.serial_number;
                rowDevice["ip_local"] = device.ip_local;
                rowDevice["port"] = device.port;
                rowDevice["comm_password"] = device.comm_password;
                rowDevice["fdid"] = device.fdid;
                rowDevice["zkclass"] = new CZKEMClass();
                rowDevice["status"] = "not_connected";
                rowDevice["assign_status"] = "not_match";

                dtDevices.Rows.Add(rowDevice);
            }

            configParam.Rows.Add(rowAdd);
        }

        private void StructuringDtDevices()
        {
            dtDevices = new DataTable();
            dtDevices.Columns.Add("serial_number");
            dtDevices.Columns.Add("ip_local");
            dtDevices.Columns.Add("port");
            dtDevices.Columns.Add("comm_password");
            dtDevices.Columns.Add("fdid");
            dtDevices.Columns.Add("zkclass", typeof(zkemkeeper.CZKEMClass));
            dtDevices.Columns.Add("status");
            dtDevices.Columns.Add("assign_status");

            /*for (int i = 0; i < 10; i++)
            {
                DataRow row = dtDevices.NewRow();
                row["zkclass"] = new CZKEMClass();
                dtDevices.Rows.Add(row);
            }*/
        }

        public void AssociateDevices(string serial_number, string ip, string port, string commPassword, string fdid)
        {
            DataRow row = dtDevices.Rows[devicesCount];
            row["serial_number"] = serial_number;
            row["ip_local"] = ip;
            row["port"] = port;
            row["comm_password"] = commPassword;
            row["fdid"] = fdid;
            //row["zkclass"] = new CZKEMClass();
            row["status"] = "not_connected";
            row["assign_status"] = "not_assigned";
            //dtDevices.Rows.Add(row);
        }

        public DataTable ConnectAllDevices(string commandID)
        {
            //default port is 4370
            foreach(DataRow device in dtDevices.Rows)
            {
                if (device["assign_status"].ToString() == "assigned")
                {
                    string ip = device["ip_local"].ToString();
                    int port = Convert.ToInt16(device["port"]);
                    int commPassword = Convert.ToInt32(device["comm_password"]);
                    int fdid = Convert.ToInt16(device["fdid"]);
                    CZKEMClass FDevice = (CZKEMClass)device["zkclass"];
                    ConsoleWriteLine("Connecting to device " + device["serial_number"].ToString() + " on " + ip + "...");
                    InCommandSocketEmit(commandID, "Please wait while connecting to device " + device["serial_number"].ToString() + " on " + AppID + "...");
                    FDevice.SetCommPassword(commPassword);

                    if (FDevice.Connect_Net(ip, port))
                    {
                        ConsoleWriteLine("Device " + device["serial_number"].ToString() + " connected on " + ip);
                        if (FDevice.RegEvent(fdid, 65535))
                        {
                            FDevice.OnFinger += new _IZKEMEvents_OnFingerEventHandler(FDevice_OnFinger);
                            FDevice.OnVerify += new _IZKEMEvents_OnVerifyEventHandler(FDevice_OnVerify);
                            FDevice.OnEnrollFingerEx += new _IZKEMEvents_OnEnrollFingerExEventHandler(FDevice_OnEnrollFingerEx);
                            //button3.Click += new EventHandler(test_click);
                        }
                        device["status"] = "connected";
                        UpdateCountDeviceLabel(dtDevices.Rows.Count.ToString());
                        //countDeviceLabel.Text = (Convert.ToInt16(countDeviceLabel.Text) + 1).ToString();
                    }
                    else
                    {
                        ConsoleWriteLine("Unable to connect device " + device["serial_number"].ToString() + " on " + ip);
                        device["status"] = "not_connected";
                    }
                }
            }

            return dtDevices;
        }

        private void connectAllDevices()
        {
            foreach (DataRow device in dtDevices.Rows)
            {

                string ip = device["ip_local"].ToString();
                int port = Convert.ToInt16(device["port"]);
                int commPassword = Convert.ToInt32(device["comm_password"]);
                int fdid = Convert.ToInt16(device["fdid"]);
                CZKEMClass FDevice = (CZKEMClass)device["zkclass"];
                ConsoleWriteLineInThread("Connecting to device " + device["serial_number"].ToString() + " on " + ip + "...");

                FDevice.SetCommPassword(commPassword);

                if (FDevice.Connect_Net(ip, port))
                {
                    ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " connected on " + ip);
                    if (FDevice.RegEvent(fdid, 65535))
                    {
                        FDevice.OnFinger += new _IZKEMEvents_OnFingerEventHandler(FDevice_OnFinger);
                        FDevice.OnVerify += new _IZKEMEvents_OnVerifyEventHandler(FDevice_OnVerify);
                        FDevice.OnEnrollFingerEx += new _IZKEMEvents_OnEnrollFingerExEventHandler(FDevice_OnEnrollFingerEx);
                        FDevice.OnAttTransactionEx += new _IZKEMEvents_OnAttTransactionExEventHandler(FDevice_OnAttTransactionEx);
                        //button3.Click += new EventHandler(test_click);
                    }
                    device["status"] = "connected";
                    UpdateCountDeviceLabel(dtDevices.Rows.Count.ToString());
                    //countDeviceLabel.Text = (Convert.ToInt16(countDeviceLabel.Text) + 1).ToString();
                    if (device["serial_number"].ToString() == GetDeviceSerialNumber(FDevice, Convert.ToInt16(device["fdid"])))
                    {
                        device["assign_status"] = "match";
                        ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " is match with configuration");
                    }
                    else
                    {
                        ConsoleWriteLineInThread("Device " + device["serial_number"].ToString() + " is not match with configuration. Contact your system administrator");
                    }
                }
                else
                {
                    ConsoleWriteLineInThread("Unable to connect device " + device["serial_number"].ToString() + " on " + ip);
                    device["status"] = "not_connected";
                }

            }
        }

        private string GetDeviceSerialNumber(CZKEMClass fdevice, int iMachineNumber)
        {
            int idwErrorCode = 0;

            string sdwSerialNumber = "";

            if (fdevice.GetSerialNumber(iMachineNumber, out sdwSerialNumber))
            {
                return sdwSerialNumber;
            }
            else
            {
                fdevice.GetLastError(ref idwErrorCode);
                return idwErrorCode.ToString();
            }
        }

        void FDevice_OnAttTransactionEx(string EnrollNumber, int IsInValid, int AttState, int VerifyMethod, int Year, int Month, int Day, int Hour, int Minute, int Second, int WorkCode)
        {
            //throw new NotImplementedException();
            ConsoleWriteLine("Transaction triggered. Verfied");
            string date = Year.ToString() + "-" + Month.ToString() + "-" + Day.ToString();
            string time = Hour.ToString() + ":" + Minute.ToString() + ":" + Second.ToString();
            string message = "{ID: " + EnrollNumber.ToString() + ", att_state: "+ AttState.ToString() +", time: " + time + "}";
            ConsoleWriteLineInThread(message);
            RTESocketIOEmit("FP_Transaction", "Verified. " + message);

            dynamic result = new JObject();
            result.command = "att_transaction";
            result.AppID = AppID;
            result.date = date;
            result.time = time;
            result.employee_number = EnrollNumber;
            result.verify_mode = VerifyMethod.ToString();
            result.in_out_mode = AttState.ToString();
            result.work_code = WorkCode.ToString();

            socket.Emit("appCommand", result);
            
        }

        void FDevice_OnEnrollFingerEx(string EnrollNumber, int FingerIndex, int ActionResult, int TemplateLength)
        {
            dynamic result = new JObject();
            if (dtEnroll.Rows.Count > 0)
            {
                result.AppID = AppID;
                result.serial_number = dtEnroll.Rows[0]["serial_number"].ToString();
                result.employee_number = dtEnroll.Rows[0]["employee_number"].ToString();
                result.commandID = dtEnroll.Rows[0]["commandID"].ToString();
                if (ActionResult == 0)
                {
                    CZKEMClass fdevice = (CZKEMClass)dtEnroll.Rows[0]["zkclass"];
                    string sTmpData = "";
                    int iTmpLength = 0;
                    int iFlag = 0;
                    fdevice.EnableDevice(Convert.ToInt16(dtEnroll.Rows[0]["fdid"]), false);
                    if (fdevice.GetUserTmpExStr(Convert.ToInt16(dtEnroll.Rows[0]["fdid"]), dtEnroll.Rows[0]["employee_number"].ToString(), 0, out iFlag, out sTmpData, out iTmpLength))
                    {
                        
                        result.fid = FingerIndex;
                        result.flag = 0;
                        result.fingerprint_tmp = sTmpData;
                        result.tmp_length = iTmpLength;
                        result.status = "success";
                        result.message = dtEnroll.Rows[0]["full_name"].ToString() + " data is successfully saved to device[" + dtEnroll.Rows[0]["serial_number"].ToString() + "]";
                    }

                    fdevice.EnableDevice(Convert.ToInt16(dtEnroll.Rows[0]["fdid"]), true);
                }
                else
                {
                    result.status = "failed";
                    result.message = "Fingerprint already existed on device " + dtEnroll.Rows[0]["serial_number"].ToString();
                }
            }
            else
            {
                result.status = "failed";
                result.message = "Process enroll failed when saving the data into device";
            }
            
            
            result.command = "enroll_complete";
            
            //dynamic msg = Newtonsoft.Json.Linq.JObject.Parse("{ eventName: 'button_click', eventArgs: [{ message: 'data from client : "+ stat +"' }] }");
            dtEnroll.Clear();
            socket.Emit("appCommand", result);
        }

        private void FDevice_OnVerify(int UserID)
        {
            /*ConsoleWriteLineInThread("Verified OK,the UserID is " + UserID.ToString());
            if (UserID != -1)
            {
                RTESocketIOEmit("Finger_On_Verify", "Verified OK,the UserID is " + UserID.ToString());
                ConsoleWriteLineInThread("Verified OK,the UserID is " + UserID.ToString());
                //data = "{ message: 'Verified OK,the UserID is " + UserID.ToString() + "', data: { userid: " + UserID.ToString() + " }";
            }
            else
            {
                RTESocketIOEmit("Finger_On_Verify", "Verified Failed");
                ConsoleWriteLineInThread("Verified Failed");
            }*/
        }

        private void FDevice_OnFinger()
        {
            ConsoleWriteLineInThread("Finger Print Triggered");
            RTESocketIOEmit("On_Finger", "Finger Print Triggered");
        }

        private bool ConnectFingerprintDevice(string ip, int port, int password)
        {
            return false;
        }

        public void ConsoleWriteLine(string message)
        {
            if (this.Console.InvokeRequired)
            {
                this.Console.Invoke(new Action(() => this.Console.Text += message + Environment.NewLine));
            }
            this.Console.Invoke(new Action(() => this.Console.SelectionStart = this.Console.Text.Length));
            this.Console.Invoke(new Action(() => this.Console.ScrollToCaret()));

        }

        public void ConsoleWriteLineInThread(string message)
        {
            this.Console.Text += message + Environment.NewLine;
            this.Console.SelectionStart = this.Console.Text.Length;
            this.Console.ScrollToCaret();
        }

        public void UpdateConnectionLabel(string message)
        {
            if (this.connectionLabel.InvokeRequired)
            {
                this.connectionLabel.Invoke(new Action(() => this.connectionLabel.Text += message));
            }
        }

        public void UpdateCountDeviceLabel(string message)
        {
            if (this.countDeviceLabel.InvokeRequired)
            {
                this.countDeviceLabel.Invoke(new Action(() => this.countDeviceLabel.Text += message));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConsoleWriteLineInThread("sending data to server");
            RTESocketIOEmit("button_click", "data from client : " + stat);
            stat++;
        }

        private void RTESocketIOEmit(string eventName, string message)
        {
            dynamic msg = new JObject();
            msg.AppID = AppID;
            msg.eventName = eventName;
            msg.eventArgs = new JArray() as dynamic;

            dynamic eArgs = new JObject();
            eArgs.messaage = message;
            msg.eventArgs.Add(eArgs);

            //dynamic msg = Newtonsoft.Json.Linq.JObject.Parse("{ eventName: 'button_click', eventArgs: [{ message: 'data from client : "+ stat +"' }] }");

            socket.Emit("rte", msg);
        }

        private void InCommandSocketEmit(string commandID, string message)
        {
            dynamic msgJson = new JObject();
            msgJson.AppID = AppID;
            msgJson.commandID = commandID;
            msgJson.message = message;

            socket.Emit("fdCommandInProcMessage", msgJson);
        }

        private void BroadcastSocketIOEmit(string message)
        {
            dynamic msg = Newtonsoft.Json.Linq.JObject.Parse("{ message: '" + message + "' }");

            socket.Emit("receiveBroadcast", msg);
        }

        public string getAppID()
        {
            return AppID;
        }

        private bool fdconnect(string ip, int port, int password, ref CZKEMClass FDevice)
        {
            FDevice.SetCommPassword(password);
            bool isConnected = FDevice.Connect_Net(ip, port);
            return isConnected;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            addDeviceForm = new AddDevice(this);
            addDeviceForm.FormClosed += new FormClosedEventHandler(addDeviceForm_FormClosed);
            addDeviceForm.ShowDialog();
        }

        void addDeviceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            foreach (DataRow device in dtDevices.Rows)
            {
                if (device["status"].ToString() == "connected")
                {
                    CZKEMClass ckDevice = (CZKEMClass)device["zkclass"];
                    ckDevice.Disconnect();
                    ckDevice.OnFinger -= new _IZKEMEvents_OnFingerEventHandler(FDevice_OnFinger);
                    ckDevice.OnVerify -= new _IZKEMEvents_OnVerifyEventHandler(FDevice_OnVerify);
                }                
            }

            ConnectDevices();
        }

        private void ConnectDevices()
        {
            foreach (DataRow device in dtDevices.Rows)
            {
                if (device["assign_status"].ToString() == "assigned")
                {
                    string ip = device["ip_local"].ToString();
                    int port = Convert.ToInt16(device["port"]);
                    int commPassword = Convert.ToInt32(device["comm_password"]);
                    int fdid = Convert.ToInt16(device["fdid"]);
                    CZKEMClass FDevice = (CZKEMClass)device["zkclass"];
                    ConsoleWriteLine("Connecting to device " + device["serial_number"].ToString() + " on " + ip + "...");
                    //InCommandSocketEmit(commandID, "Please wait while connecting to device " + device["serial_number"].ToString() + " on " + AppID + "...");
                    FDevice.SetCommPassword(commPassword);
                    
                    if (FDevice.Connect_Net(ip, port))
                    {
                        ConsoleWriteLine("Device " + device["serial_number"].ToString() + " connected on " + ip);
                        if (FDevice.RegEvent(fdid, 65535))
                        {
                            FDevice.OnFinger += new _IZKEMEvents_OnFingerEventHandler(FDevice_OnFinger);
                            FDevice.OnVerify += new _IZKEMEvents_OnVerifyEventHandler(FDevice_OnVerify);
                            //button3.Click += new EventHandler(test_click);
                        }
                        device["status"] = "connected";
                        UpdateCountDeviceLabel(dtDevices.Rows.Count.ToString());
                        //countDeviceLabel.Text = (Convert.ToInt16(countDeviceLabel.Text) + 1).ToString();
                    }
                    else
                    {
                        ConsoleWriteLine("Unable to connect device " + device["serial_number"].ToString() + " on " + ip);
                        device["status"] = "not_connected";
                    }
                }
            }
        }

        public dynamic GetTranscationAll()
        {
            dynamic result = new JObject();
            result.command = "request_transaction";
            dynamic devices = new JArray() as dynamic;

            string sdwEnrollNumber = "";
            int idwVerifyMode = 0;
            int idwInOutMode = 0;
            int idwYear = 0;
            int idwMonth = 0;
            int idwDay = 0;
            int idwHour = 0;
            int idwMinute = 0;
            int idwSecond = 0;
            int idwWorkcode = 0;

            foreach (DataRow device in dtDevices.Rows)
            {   
                dynamic dev = new JObject();
                dev.serial_number = device["serial_number"];
                dev.transaction = new JArray() as dynamic;

                CZKEMClass fdevice = device["zkclass"] as CZKEMClass;
                if (fdevice.ReadGeneralLogData(Convert.ToInt16(device["fdid"])))//read all the attendance records to the memory
                {
                    while (fdevice.SSR_GetGeneralLogData(Convert.ToInt16(device["fdid"]), out sdwEnrollNumber, out idwVerifyMode,
                               out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                    {
                        dynamic transdata = new JObject();
                        transdata.date = idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString();
                        transdata.time = idwHour.ToString() + ":" + idwMinute.ToString() + ":" + idwSecond.ToString();
                        transdata.employee_number = sdwEnrollNumber;
                        transdata.verify_mode = idwVerifyMode.ToString();
                        transdata.in_out_mode = idwInOutMode.ToString();
                        transdata.work_code = idwWorkcode.ToString();
                        transdata.AppID = AppID;
                        transdata.serial_number = device["serial_number"];
                        dev.transaction.Add(transdata);
                    }
                }
                else
                {
                    dev.transaction_status = "failed";
                }
                dev.transaction_status = "success";
                devices.Add(dev);
                fdevice.EnableDevice(Convert.ToInt16(device["fdid"]), true);//enable the device
            }
            result.data = devices;

            return result;

        }

        public dynamic EnrollUser(string employeeNumber, string name, string serialNumber, string commandID)
        {
            

            dynamic responStatus = new JObject();
            DataRow device = getDeviceBySerialNumber(serialNumber);
            
            if (device != null)
            {
                if (device["status"] == "connected")
                {
                    if (dtEnroll.Rows.Count == 0)
                    {
                        CZKEMClass fdevice = (CZKEMClass)device["zkclass"];

                        DataRow rowEnroll = dtEnroll.NewRow();
                        rowEnroll["serial_number"] = serialNumber;
                        rowEnroll["employee_number"] = employeeNumber;
                        rowEnroll["commandID"] = commandID;
                        rowEnroll["zkclass"] = fdevice;
                        rowEnroll["full_name"] = name;
                        rowEnroll["fdid"] = device["fdid"];
                        dtEnroll.Rows.Add(rowEnroll);

                        InCommandSocketEmit(commandID, "Preparing the device[" + serialNumber + "] for enrolling ");

                        //Set UserInfo First
                        if (fdevice.SSR_SetUserInfo(Convert.ToInt16(device["fdid"]), employeeNumber, name, null, 0, true))
                        {
                            //Remove existing FP template data
                            if (fdevice.SSR_DelUserTmpExt(Convert.ToInt16(device["fdid"]), employeeNumber, 0))
                            {
                                fdevice.RefreshData(Convert.ToInt16(device["fdid"]));//the data in the device should be refreshed
                                InCommandSocketEmit(commandID, "Preparation on device[" + serialNumber + "] is complete");


                                //Start Enrolling
                                if (fdevice.StartEnrollEx(employeeNumber, 0, 0))
                                {
                                    InCommandSocketEmit(commandID, "Start enrolling fingerprint on device[" + serialNumber + "] for User: " + name + " with ID: " + employeeNumber);
                                    fdevice.StartIdentify();//After enrolling templates,you should let the device into the 1:N verification condition
                                    responStatus.status = "success";
                                    responStatus.message = "Waiting for the user enroll fingerprint...";
                                }
                                else
                                {
                                    int errCode = 0;
                                    fdevice.GetLastError(ref errCode);
                                    responStatus.status = "failed";
                                    responStatus.message = "Operation failed,ErrorCode=" + errCode.ToString() + " on the device[" + serialNumber + "]";
                                    dtEnroll.Clear();
                                }
                            }
                            else
                            {
                                responStatus.status = "failed";
                                responStatus.message = "failed to remove existing user on the device[" + serialNumber + "]";
                                dtEnroll.Clear();
                            }
                        }
                        else
                        {
                            responStatus.status = "failed";
                            responStatus.message = "failed to set user on the device[" + serialNumber + "]";
                            dtEnroll.Clear();
                        }
                    }
                    else
                    {
                        responStatus.status = "failed";
                        responStatus.message = "device with serial " + serialNumber + " is locked by another enroll process";
                    }
                }
                else
                {
                    responStatus.status = "failed";
                    responStatus.message = "device with serial " + serialNumber + " is not connected with fingerprint daemon " + AppID;
                }
            }
            else
            {
                responStatus.status = "failed";
                responStatus.message = "device with serial " + serialNumber + "not found" + AppID;
                dtEnroll.Clear();
            }

           
            return responStatus;
        }

        public dynamic RegisterUserBulk(dynamic Employees, string serialNumber, string commandID)
        {
            dynamic result = new JObject();
            result.command = "register_user_bulk";
            result.employee_result = new JArray() as dynamic;
            JArray employees = (JArray)JsonConvert.DeserializeObject(Convert.ToString(Employees.employee_assign));

            DataRow device = getDeviceBySerialNumber(serialNumber);
            bool registerFailed = false;

            if (device != null)
            {
                if (device["status"] == "connected")
                {
                    CZKEMClass fdevice = (CZKEMClass)device["zkclass"];
                    InCommandSocketEmit(commandID, "Preparing the device[" + serialNumber + "] for registering employees ");

                    int iMachineNumber = Convert.ToInt16(device["fdid"]);
                    string sdwEnrollNumber = "";
                    string sName = "";
                    int idwFingerIndex = 0;
                    string sTmpData = "";
                    int iPrivilege = 0;
                    string sPassword = "";
                    bool bEnabled = true;
                    int iFlag = 1;
                    int iUpdateFlag = 1;

                    //Clearing the user data in the device

                    if (fdevice.ClearData(iMachineNumber, 5))
                    {
                        fdevice.RefreshData(iMachineNumber);//the data in the device should be refreshed
                        InCommandSocketEmit(commandID, "Successfully clear all user data in the device[" + serialNumber + "]");
                    }
                    else
                    {
                        InCommandSocketEmit(commandID, "Failed to clear all user data in the device[" + serialNumber + "]");
                    }

                    //Clear the user's fingerprint template in device

                    if (fdevice.ClearData(iMachineNumber, 2))
                    {
                        fdevice.RefreshData(iMachineNumber);//the data in the device should be refreshed
                        InCommandSocketEmit(commandID, "Successfully clear all user's fingerprint data in the device[" + serialNumber + "]");
                    }
                    else
                    {
                        InCommandSocketEmit(commandID, "Failed to clear all user's fingerprint data in the device[" + serialNumber + "]");
                    }

                    fdevice.EnableDevice(iMachineNumber, false);
                    if (fdevice.BeginBatchUpdate(iMachineNumber, iUpdateFlag))//create memory space for batching data
                    {
                        foreach (var empl in employees)
                        {
                            dynamic employee = new JObject();
                            employee.id_employee = empl["so_assignment_number"];
                            employee.employee_number = empl["employee_number"];

                            InCommandSocketEmit(commandID, "Begin registering employee : " + empl["full_name"].ToString() + " with ID: " + empl["employee_number"].ToString() + " on device[" + serialNumber + "]");
                            if (empl["fp_status"].ToString() == "fp_not_exist")
                            {
                                InCommandSocketEmit(commandID, "Employee : " + empl["full_name"].ToString() + " with ID: " + empl["employee_number"].ToString() + " doesn't have fingerprint registered on database. You need to enroll fingerprint for this user");
                                employee.fp_assign_status = "need_enroll";
                                registerFailed = true;
                            }
                            else
                            {
                                //Register the user in device
                                sdwEnrollNumber = empl["employee_number"].ToString();
                                sName = empl["full_name"].ToString();
                                sTmpData = empl["fingerprint_tmp"].ToString();

                                if (fdevice.SSR_SetUserInfo(iMachineNumber, sdwEnrollNumber, sName, sPassword, iPrivilege, bEnabled))//upload user information to the memory
                                {
                                    fdevice.SetUserTmpExStr(iMachineNumber, sdwEnrollNumber, idwFingerIndex, iFlag, sTmpData);//upload templates information to the memory
                                    employee.fp_assign_status = "register_success";
                                }
                                else
                                {
                                    employee.fp_assign_status = "register_failed";
                                    registerFailed = true;
                                }

                            }

                            fdevice.BatchUpdate(iMachineNumber);//upload all the information in the memory
                            fdevice.RefreshData(iMachineNumber);//the data in the device should be refreshed
                            fdevice.EnableDevice(iMachineNumber, true);

                            result.employee_result.Add(employee);
                        }

                        if (registerFailed)
                        {
                            result.status = "success_with_detail_failed";
                            result.message = "Successfully register some employees to device[" + serialNumber + "] on " + AppID;
                        }
                        else
                        {
                            result.status = "success_with_detail_failed";
                            result.message = "Successfully register all employees to device[" + serialNumber + "] on " + AppID;
                        }
                    }
                    else
                    {
                        result.status = "failed";
                        result.message = "Error when trying to reserve memory on device[" + serialNumber + "]";
                    }
                }
                else
                {
                    result.status = "failed";
                    result.message = "device with serial " + serialNumber + " is not connected with fingerprint daemon " + AppID;
                }
            }
            else
            {
                result.status = "failed";
                result.message = "device with serial " + serialNumber + "not found on " + AppID;
            }

            return result;
        }

        private DataRow getDeviceBySerialNumber(string serialNumber)
        {
            DataRow returnRow = null;
            foreach (DataRow device in dtDevices.Rows)
            {
                if (device["serial_number"].ToString() == serialNumber)
                {
                    returnRow = device;
                    break;
                }
            }

            return returnRow;
        }
    }
}

