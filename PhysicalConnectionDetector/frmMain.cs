using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace physicalConnectionDetector
{
    public partial class FormMain : Form
    {
        readonly char[] units = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        public FormMain()
        {
            InitializeComponent();
        }

        //Data Structure that stores the handle of connections
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_VOLUME
        {
            public int dbcv_size;
            public int dbcv_devicetype;
            public int dbcv_reserved;
            public int dbcv_unitmask;
        }

        /// <summary>
        /// Overides method to manage the arrival of new disk drives
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // These definitions are in dbt.h and winuser.h

            // A change has occurred in the devices
            const int WM_DEVICECHANGE = 0x0219;
            // The system detects a new device
            const int DBT_DEVICEARRIVAL = 0x8000;
            // Request removal of the device
            const int DBT_DEVICEQUERYREMOVE = 0x8001;
            // Device removal failed
            const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
            // Pending device removal
            const int DBT_DEVICEREMOVEPENDING = 0x8003;
            // Device removed from the system
            const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
            // Logical volume (A disk has been inserted)
            const int DBT_DEVTYP_VOLUME = 0x00000002;

            string logToAdd = string.Empty;

            switch (m.Msg)
            {
                // Change system devices
                case WM_DEVICECHANGE:
                    switch (m.WParam.ToInt32())
                    {
                        //Device connected
                        case DBT_DEVICEARRIVAL:
                            {
                                int devType = Marshal.ReadInt32(m.LParam, 4);
                                //It's a logical volume
                                if (devType == DBT_DEVTYP_VOLUME)
                                {
                                    DEV_BROADCAST_VOLUME vol;
                                    vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME));

                                    logToAdd = string.Format("{0} - {1} - Disk drive inserted: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), UnitTag(vol.dbcv_unitmask));
                                    txtLog.AppendText(logToAdd + Environment.NewLine);
                                }
                            }
                            break;
                        case DBT_DEVICEREMOVECOMPLETE:
                            logToAdd = string.Format("{0} - {1} - Device removed.", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                            txtLog.AppendText(logToAdd + Environment.NewLine);
                            break;
                    }
                    break;
            }

            // Now use the default handler
            base.WndProc(ref m);
        }

        /// <summary>
        /// Detect the tag asigned to the unit
        /// </summary>
        /// <param name="unitmask"></param>
        /// <returns></returns>
        private char UnitTag(int unitmask)
        {
            int i = 0;

            // We convert the mask into a primary array and look for the index of the first occurrence (the drive letter)
            BitArray bitArrayMask = new BitArray(System.BitConverter.GetBytes(unitmask));
            foreach (bool item in bitArrayMask)
            {
                if (item == true)
                {
                    break;
                }
                i++;
            }
            return units[i];
        }

        private void FrmMain_Resize(object sender, System.EventArgs e)
        {
            //When the form is minimized we want hide in system Tray represented by the NotifyIcon control
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }
    }
}
