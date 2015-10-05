using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EdgeJs;
using Newtonsoft;
using Newtonsoft.Json;
using Microsoft.Win32;
using SocketIOClient;
using Newtonsoft.Json.Linq;
using System.Data;


namespace FingerprintSync
{
    static class Program
    {
        static Form1 myForm;
        static Client socket;
        static DataTable dtCommand;
        static DataTable ConfigParam;
        static dynamic deviceJson;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            StructuringCommandDatatable();
            deviceJson = ReadFileConfiguration("fd_configuration.conf");

            myForm = new Form1(deviceJson);

            connectSocket();

            Application.Run(myForm);
        }

        static void connectSocket()
        {
            try
            {
                socket = new Client(ConfigParam.Rows[0]["Host"].ToString());
                socket.Error += socket_Error;
                socket.Opened += socket_Opened;
                socket.ConnectionRetryAttempt += socket_ConnectionRetryAttempt;
                socket.SocketConnectionClosed += socket_SocketConnectionClosed;
                myForm.Socket = socket;
                socket.On("welcome", (message) =>
                {
                    //MessageBox.Show(data.RawMessage);
                    String msg = message.Json.Args[0].ToString();
                    dynamic json = JObject.Parse(msg);
                    Console.Write(json.message);
                    myForm.ConsoleWriteLine(json.message.ToString());

                    dynamic respon = JObject.Parse("{AppID: '" + ConfigParam.Rows[0]["AppID"].ToString() + "'}");

                    socket.Emit("regAppID", respon);
                    //MessageBox.Show(msg, "Received Data");
                });

                socket.On("time", (data) =>
                {
                    //MessageBox.Show(data.RawMessage);
                    String msg = data.Json.Args[0].ToString();
                    dynamic json = JObject.Parse(msg);

                    Console.Write(json.time);
                    myForm.ConsoleWriteLine(json.time.ToString());
                    socket.Emit("i am client", "{data: '" + msg + "'}");
                    //MessageBox.Show(msg, "Received Data");
                });

                socket.On("dMessage", (data) =>
                {
                    //MessageBox.Show(data.RawMessage);
                    String msg = data.Json.Args[0].ToString();
                    dynamic json = JObject.Parse(msg);

                    myForm.ConsoleWriteLine(json.message.ToString());
                    //socket.Emit("i am client", "{data: '" + msg + "'}");
                    //MessageBox.Show(msg, "Received Data");
                });

                socket.On("request_transaction", (data) =>
                {
                    String msg = data.Json.Args[0].ToString();
                    dynamic json = JObject.Parse(msg);
                    myForm.ConsoleWriteLine("Get transaction on date : " + json.date.ToString());
                    ExecuteCommand("", "request_transaction", "");
                });

                socket.On("fdCommand", (data) =>
                {
                    String msg = data.Json.Args[0].ToString();
                    dynamic json = JObject.Parse(msg);

                    myForm.ConsoleWriteLine("command : " + json.command.ToString() + " ; parameter : " + json.parameter.ToString());

                    //dynamic parameter = JObject.Parse(json.parameter.ToString());
                    //myForm.ConsoleWriteLine("device : " + parameter.device.ToString());

                    //Store command to command datatable
                    AddCommandLine(json.commandID.ToString(), json.command.ToString(), json.parameter.ToString(), "not_exec");
                    ExecuteCommand(json.commandID.ToString(), json.command.ToString(), json.parameter.ToString());


                    /*dynamic responJson = new JObject();
                    responJson.commandID = "";
                    responJson.appID = myForm.getAppID();
                    responJson.respons = json.command.ToString();

                    

                    socket.Emit("fdCommandRespons", responJson);*/
                });

                socket.Connect();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Something Went Wrong!!");
                Application.Exit();
            }

            if (socket.ReadyState == WebSocket4Net.WebSocketState.None)
            {
                socket.Dispose();
                socket.Error -= socket_Error;
                socket.Opened -= socket_Opened;
                socket.ConnectionRetryAttempt -= socket_ConnectionRetryAttempt;
                socket.SocketConnectionClosed -= socket_SocketConnectionClosed;
                myForm.Socket.Dispose();
                myForm.Socket = null;
                socket = null;
                
                connectSocket();
            } 
        }

        static void socket_SocketConnectionClosed(object sender, EventArgs e)
        {
            myForm.ConsoleWriteLine("Socket Closed");
            connectSocket();
        }

        static void socket_ConnectionRetryAttempt(object sender, EventArgs e)
        {
            myForm.ConsoleWriteLine("Reconnecting...");
        }

        static void AddCommandLine(string commandid, string command, string parameter, string status)
        {
            DataRow row = dtCommand.NewRow();
            row["commandID"] = commandid;
            row["command"] = command;
            row["parameter"] = parameter;
            row["status"] = status;
            row["respons"] = "";

            dtCommand.Rows.Add(row);
        }

        static void ChangeStatusCommandById(string commandid, string status)
        {
            foreach (DataRow row in dtCommand.Rows)
            {
                if (row["commandID"].ToString() == commandid)
                {
                    row["status"] = status;
                }
            }
        }

        static void StructuringCommandDatatable()
        {
            dtCommand = new DataTable();
            dtCommand.Columns.Add("commandID");
            dtCommand.Columns.Add("command");
            dtCommand.Columns.Add("parameter");
            dtCommand.Columns.Add("status");
            dtCommand.Columns.Add("respons");

        }

        static void socket_Opened(object sender, EventArgs e)
        {
            myForm.ConsoleWriteLine("Connection to socket IO is open");
            myForm.UpdateConnectionLabel(ConfigParam.Rows[0]["Host"].ToString());
        }

        static void socket_Error(object sender, ErrorEventArgs e)
        {
            myForm.ConsoleWriteLine(e.Message.ToString());
        }

        static async void ExecuteCommand(string commandid, string command, string paramater)
        {
            
            switch (command)
            {
                case "connect_device":
                    var paramJson = JToken.Parse(paramater);
                    var deviceList = paramJson.Children<JProperty>().FirstOrDefault(x => x.Name == "devices").Value;

                    //myForm.DtDevices.Clear();
                    foreach (var device in deviceList.Children())
                    {
                        var item = device.Children<JProperty>();
                        string serial_number = item.FirstOrDefault(x => x.Name == "serial_number").Value.ToString();
                        string ip = item.FirstOrDefault(x => x.Name == "ip_local").Value.ToString();
                        string port = item.FirstOrDefault(x => x.Name == "port").Value.ToString();
                        string commPassword = item.FirstOrDefault(x => x.Name == "comm_password").Value.ToString();
                        string fdid = item.FirstOrDefault(x => x.Name == "fdid").Value.ToString();
                        myForm.AssociateDevices(serial_number, ip, port, commPassword, fdid);

                    }

                    DataTable result = await Task.Run(() => myForm.ConnectAllDevices(commandid));

                    JObject respon = new JObject();
                    JArray dev = new JArray();
                    foreach (DataRow device in result.Rows)
                    {
                        dev.Add(new JObject(
                            new JProperty("serial_number", device["serial_number"]),
                            new JProperty("fdid", device["fdid"]),
                            new JProperty("status", device["status"])
                        ));
                    }

                    respon.Add(new JProperty("AppID",myForm.getAppID()));
                    respon.Add(new JProperty("commandID", commandid));
                    respon.Add(new JProperty("command", command));
                    respon.Add(new JProperty("respons", dev));

                    ChangeStatusCommandById(commandid, "exec");

                    SendResponsFdCommand((dynamic)respon);
                    break;
                case "request_transaction":
                    dynamic returnJson = await Task.Run(() => myForm.GetTranscationAll());
                    socket.Emit("appCommand", returnJson);

                    break;
                case "enroll_fingerprint":
                    dynamic paramJsonEnroll = JObject.Parse(paramater);
                    dynamic resultEnroll = await Task.Run(() => myForm.EnrollUser(paramJsonEnroll.employee_number.ToString(), paramJsonEnroll.full_name.ToString(), paramJsonEnroll.serial_number.ToString(), commandid));
                    JObject responJsonEnroll = new JObject();

                    responJsonEnroll.Add(new JProperty("AppID", myForm.getAppID()));
                    responJsonEnroll.Add(new JProperty("commandID", commandid));
                    responJsonEnroll.Add(new JProperty("command", command));
                    responJsonEnroll.Add(new JProperty("respons", resultEnroll));

                    ChangeStatusCommandById(commandid, "exec");

                    SendResponsFdCommand((dynamic)responJsonEnroll);
                    break;
                case "register_user_bulk":
                    dynamic paramJsonRegister = JObject.Parse(paramater);

                    dynamic resultRegister = await Task.Run(() => myForm.RegisterUserBulk(paramJsonRegister, paramJsonRegister.serial_number.ToString(), commandid));

                    JObject responJsonRegister = new JObject();

                    responJsonRegister.Add(new JProperty("AppID", myForm.getAppID()));
                    responJsonRegister.Add(new JProperty("commandID", commandid));
                    responJsonRegister.Add(new JProperty("command", command));
                    responJsonRegister.Add(new JProperty("respons", resultRegister));

                    ChangeStatusCommandById(commandid, "exec");

                    SendResponsFdCommand((dynamic)responJsonRegister);

                    break;
                default:
                    JObject responJson = new JObject();

                    responJson.Add(new JProperty("AppID",myForm.getAppID()));
                    responJson.Add(new JProperty("commandID", commandid));
                    responJson.Add(new JProperty("command", command));
                    responJson.Add(new JProperty("respons", "command_not_valid"));

                    ChangeStatusCommandById(commandid, "exec");

                    SendResponsFdCommand((dynamic)responJson);
                    break;
            }
        }

        static void SendResponsFdCommand(dynamic respon)
        {
            socket.Emit("fdCommandRespons", respon);
        }

        static dynamic ReadFileConfiguration(string filename)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);

            string configText = reader.ReadToEnd();
            dynamic configJson = JObject.Parse(configText);

            ConfigParam = new DataTable();
            ConfigParam.Columns.Add("AppID");
            ConfigParam.Columns.Add("Host");

            ConfigParam.Columns.Add("devices", typeof(System.Data.DataTable));

            DataRow rowAdd = ConfigParam.NewRow();
            rowAdd["AppID"] = configJson.AppID;
            rowAdd["Host"] = configJson.Host;

            ConfigParam.Rows.Add(rowAdd);

            reader.Close();

            return configJson;
        }
    }
}
