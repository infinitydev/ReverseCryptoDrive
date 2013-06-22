using ByteStorm.PassthroughDrive;
using Dokan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ByteStorm.ReverseCryptoDrive.Gui
{
    public partial class ReverseCryptoDrive : Form
    {
        private bool isMounted = false;
        public ReverseCryptoDrive()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            string path = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_MOUNTPATH, "");
            string driveLetter = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_DRIVELETTER, CryptoConfiguration.DEFAULT_DRIVELETTER);
            textBoxMountPath.Text = path;
            driveLetterBox.Text = driveLetter;
        }

        private void Mount()
        {
            if (isMounted)
                MessageBox.Show("Drive is already mounted or mounting is in progress.");
            else
            {
                saveSettings();
                isMounted = true;
                Thread mountThread = new Thread(this.AsyncMount);
                mountThread.Start();
            }
        }

        private void saveSettings()
        {
            string oldDriveLetter = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_DRIVELETTER, CryptoConfiguration.DEFAULT_DRIVELETTER);
            string oldMountPath = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_MOUNTPATH, "");

            bool settingChanged = false;
            if (driveLetterBox.Text.Length > 0 && !driveLetterBox.Text.Equals(oldDriveLetter))
            {
                CryptoConfiguration.Instance.setSetting(CryptoConfiguration.KEY_DRIVELETTER, driveLetterBox.Text.Substring(0, 1));
                settingChanged = true;
            }
            if (!textBoxMountPath.Text.Equals(oldMountPath))
            {
                CryptoConfiguration.Instance.setSetting(CryptoConfiguration.KEY_MOUNTPATH, textBoxMountPath.Text);
                settingChanged = true;
            }

            if (settingChanged)
            {
                CryptoConfiguration.Instance.save();
            }
        }

        public void AsyncMount()
        {
            try
            {
                DokanOptions opt = new DokanOptions();
                opt.MountPoint = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_DRIVELETTER, CryptoConfiguration.DEFAULT_DRIVELETTER);
                opt.DebugMode = false;
                opt.UseStdErr = false;
                opt.VolumeLabel = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_VOLUMELABEL, "ByteStormDrive");
                string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ByteStorm", "CryptoDrive", "mappings.db");

                byte[] key, iv;

                if (loadKeyAndIV(out key, out iv))
                {
                    string mountPath = loadMountPath();
                    Console.WriteLine("Mounting " + mountPath + " as drive " + opt.MountPoint + ":");
                    CryptViewOperations cwo = new CryptViewOperations(mountPath, dbpath, key, iv);

                    Invoke((Action)delegate { Mounted(); });
                    int status = DokanNet.DokanMain(opt, cwo);
                    switch (status)
                    {
                        case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                            Invoke((Action)delegate { MessageBox.Show(this, "The drive letter is already taken.", "Mounting failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });
                            Console.WriteLine("Drive letter error");
                            break;
                        case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                            Invoke((Action)delegate { MessageBox.Show(this, "Device driver could not be installed.", "Mounting failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });
                            Console.WriteLine("Driver install error");
                            break;
                        case DokanNet.DOKAN_MOUNT_ERROR:
                            Invoke((Action)delegate { MessageBox.Show(this, "Mounting failed for some unknown reason.", "Mounting failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });
                            Console.WriteLine("Mount error");
                            break;
                        case DokanNet.DOKAN_START_ERROR:
                            Invoke((Action)delegate { MessageBox.Show(this, "Could not start up for some unknown reason.", "Mounting failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });
                            Console.WriteLine("Start error");
                            break;
                        case DokanNet.DOKAN_ERROR:
                            Invoke((Action)delegate { MessageBox.Show(this, "Some unknown error occurred.", "Mounting failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });
                            Console.WriteLine("Unknown error");
                            break;
                        case DokanNet.DOKAN_SUCCESS:
                            Console.WriteLine("Drive unmounted");
                            break;
                        default:
                            Console.WriteLine(string.Format("Unknown status: %d", status));
                            break;
                    }
                    cwo.shutdown();
                }
                else
                {
                    Invoke((Action)delegate { MessageBox.Show(this, "Could not load or generate crypto keys.", "Mounting failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });
                    Console.WriteLine("Unable to load crypto keys.");
                }
            }
            finally
            {
                Invoke((Action)delegate { Unmounted(); });
            }
        }

        public void AsyncUnmount()
        {
            string driveletter = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_DRIVELETTER, CryptoConfiguration.DEFAULT_DRIVELETTER);
            if (driveletter != null && driveletter.Length > 0)
            {
                DokanNet.DokanUnmount(driveletter[0]);
                DokanNet.DokanRemoveMountPoint(driveletter);
            }
            else
                MessageBox.Show(this, "Illegal drive letter.", "Unmounting failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static string loadMountPath()
        {
            string path = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_MOUNTPATH, null);
            if (path == null || !(new DirectoryInfo(path).Exists))
            {
                return null;
            }
            return path;
        }

        static bool loadKeyAndIV(out byte[] key, out byte[] iv)
        {
            System.Console.WriteLine("Settings file: " + getSettingsFilePath());

            string keyString = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_KEY, null);//Properties.Settings.Default.key;
            string ivString = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_IV, null);// Properties.Settings.Default.iv;

            if ((keyString == null || keyString.Length == 0) && (ivString == null || ivString.Length == 0))
            {
                System.Console.WriteLine("Generating new crypto key and initialization vector...");
                Random rnd = new Random();
                key = generateKey(rnd);
                rnd.NextBytes(new byte[rnd.Next(128 * 1024)]);
                iv = generateIV(rnd);
                return saveKeyAndIv(key, iv);
            }
            else
            {
                key = convertToByteArray(keyString, CryptoConstants.CIPHER_KEY_SIZE);
                iv = convertToByteArray(ivString, CryptoConstants.CIPHER_BLOCK_SIZE);
                if (iv == null || key == null)
                {
                    System.Console.WriteLine("Illegal key/iv format in settings file");
                    return false;
                }
                System.Console.WriteLine("Loaded key/iv from settings file");
            }
            return true;
        }

        private static string getSettingsFilePath()
        {
            return CryptoConfiguration.configPath;
        }

        private static byte[] convertToByteArray(string s, int expectedByteLength)
        {
            try
            {
                byte[] bytes = System.Convert.FromBase64String(s);
                if (bytes.Length != expectedByteLength)
                    return null;
                return bytes;
            }
            catch
            {
                return null;
            }

        }

        private static bool saveKeyAndIv(byte[] key, byte[] iv)
        {
            try
            {
                string keyString = System.Convert.ToBase64String(key);
                string ivString = System.Convert.ToBase64String(iv);
                CryptoConfiguration.Instance.setSetting(CryptoConfiguration.KEY_KEY, keyString);
                CryptoConfiguration.Instance.setSetting(CryptoConfiguration.KEY_IV, ivString);
                CryptoConfiguration.Instance.save();
                return true;
            }
            catch
            {
                Console.WriteLine("Key conversion error");
                return false;
            }
        }

        private static byte[] generateIV(Random rng)
        {
            byte[] iv = new byte[CryptoConstants.CIPHER_BLOCK_SIZE];
            rng.NextBytes(iv);
            return iv;
        }

        private static byte[] generateKey(Random rng)
        {
            byte[] key = new byte[CryptoConstants.CIPHER_KEY_SIZE];
            rng.NextBytes(key);
            return key;
        }

        private void Mounted()
        {
            isMounted = true;
            toolStripStatus.Text = "mounted";
            updateButtonStates();
        }

        private void updateButtonStates()
        {
            buttonMount.Enabled = !isMounted;
            buttonUnmount.Enabled = chkForce.Checked || isMounted;
            mountToolStripMenuItem.Enabled = !isMounted;
            unmountToolStripMenuItem.Enabled = chkForce.Checked || isMounted;
        }

        private void Unmount()
        {
            if (!isMounted && !chkForce.Checked)
                MessageBox.Show("Drive is not mounted");
            else
            {
                Thread unmountThread = new Thread(this.AsyncUnmount);
                unmountThread.Start();
            }
        }

        private void Unmounted()
        {
            isMounted = false;
            toolStripStatus.Text = "not mounted";
            updateButtonStates();
        }

        private void buttonMount_Click(object sender, EventArgs e)
        {
            Mount();
        }

        private void buttonUnmount_Click(object sender, EventArgs e)
        {
            Unmount();
        }

        private void mountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mount();
        }

        private void unmountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Unmount();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ReverseCryptoDrive_Load(object sender, EventArgs e)
        {
            // Instantiate the writer
            TextWriter  writer = new TextBoxStreamWriter(textBoxLogging);
            // Redirect the out Console stream
            Console.SetOut(writer);
        }

        private void chkForce_CheckedChanged(object sender, EventArgs e)
        {
            updateButtonStates();
        }

        private void ReverseCryptoDrive_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isMounted)
            {
                MessageBox.Show(this, "The drive is still mounted. Please unmount it first.");
                e.Cancel = true;
            }
        }

        private void ReverseCryptoDrive_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

    }
}
