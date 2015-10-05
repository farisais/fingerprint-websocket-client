using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zkemkeeper;

namespace FingerprintSync
{
    public partial class AddDevice : Form
    {
        private Form1 parentForm;
        public AddDevice(Form1 parent)
        {
            InitializeComponent();

            parentForm = parent;
            foreach (DataRow row in parentForm.DtDevices.Rows)
            {
                dataGridViewFP.Rows.Add();
                
                int indexLast = dataGridViewFP.Rows.Count;
                dataGridViewFP.Rows[indexLast - 1].Cells["serial_number"].Value = row["serial_number"].ToString();
                dataGridViewFP.Rows[indexLast - 1].Cells["ip_local"].Value = row["ip_local"].ToString();
                dataGridViewFP.Rows[indexLast - 1].Cells["fdid"].Value = row["fdid"].ToString();
                dataGridViewFP.Rows[indexLast - 1].Cells["status"].Value = row["status"].ToString();
            }
        }

        private void AddDeviceButton_Click(object sender, EventArgs e)
        {
            //dataGridViewFP.Rows.Add();
        }

        private void RemoveDeviceButton_Click(object sender, EventArgs e)
        {
            /*if (dataGridViewFP.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewFP.SelectedRows)
                {
                    dataGridViewFP.Rows.Remove(row);
                }
            }
            else
            {
                MessageBox.Show("Please select row you want to delete first");
            }*/
        }

        private void AddDevice_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*parentForm.DtDevices.Rows.Clear();
            foreach (DataGridViewRow row in dataGridViewFP.Rows)
            {
                DataRow rowAdd = parentForm.DtDevices.NewRow();
                rowAdd["serial_number"] = row.Cells["serial_number"].Value;
                rowAdd["ip_local"] = row.Cells["ip_local"].Value;
                //rowAdd["comm_password"] = row.Cells["comm_password"].Value;
                rowAdd["port"] = row.Cells["port"].Value;
                rowAdd["fdid"] = row.Cells["fdid"].Value;
                rowAdd["zkclass"] = new CZKEMClass();
                rowAdd["status"] = "not_connected";
                rowAdd["assign_status"] = "not_assigned";
                parentForm.DtDevices.Rows.Add(rowAdd);
            }*/
        }
    }
}
