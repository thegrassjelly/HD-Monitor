using System;
using System.Drawing;
using System.Windows.Forms;
using System.Management;
using System.Threading;

namespace HD_Monitor
{
    public partial class HiddenForm : Form
    {
        #region Global vars
        private NotifyIcon hddNotifyIcon;
        private Icon busyIcon;
        private Icon idleIcon;
        private Thread hddInfoWorkerThread;
        #endregion

        #region Main Form
        public HiddenForm()
        {
            InitializeComponent();

            // Load icons from files into objects
            busyIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

            // Create notify icons and assign idle icon and show it
            hddNotifyIcon = new NotifyIcon();
            hddNotifyIcon.Icon = idleIcon;
            hddNotifyIcon.Visible = true;

            // Menu items object init
            var progNameMenuItem = new MenuItem("HDD Monitor v1.0 Beta by: Steven Tomas");
            var quitMenuItem = new MenuItem("Quit");
            var contextMenu = new ContextMenu();

            // Add menu items to context menu
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);

            // Add contextmenu to notify tray app
            hddNotifyIcon.ContextMenu = contextMenu;

            // Wire up quit button to close application
            quitMenuItem.Click += quitMenuItem_Click;

            // Hide the form, this is a notification tray app
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Start worker thread that pulls HDD activity
            hddInfoWorkerThread = new Thread(new ThreadStart(HddActivityThread));
            hddInfoWorkerThread.Start();
        }
        #endregion

        #region Context Menu Event Handlers
        // Close application on click of quit menu item
        private void quitMenuItem_Click(object sender, EventArgs e)
        {
            // End task
            hddInfoWorkerThread.Abort();
            hddNotifyIcon.Dispose();
            this.Close();
        }
        #endregion

        #region HD activity threads
        // This is the thread that pulls the HDD for activity and updates the notif icon
        public void HddActivityThread()
        {
            var driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            try
            {
                // Main run loop
                while (true)
                {
                    // Connec to the drive performance instance
                    var driveDataClassCollection = driveDataClass.GetInstances();
                    foreach (ManagementObject obj in driveDataClassCollection)
                    {
                        // Only pricess the _Total instance and ignore all the individual instances
                        if (obj["Name"].ToString() == "_Total")
                        {
                            // Convert to unsined 64bit int based on wbemtest
                            if (Convert.ToUInt64(obj["DiskBytesPersec"]) > 0)
                            {
                                // Show busy icon
                                hddNotifyIcon.Icon = busyIcon;
                            }
                            else
                            {
                                // Show idle icon
                                hddNotifyIcon.Icon = idleIcon;
                            }
                        }
                    }

                    // Sleep for 10th of ms
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException tbe)
            {
                driveDataClass.Dispose();
            }
        }
        #endregion
    }
}
