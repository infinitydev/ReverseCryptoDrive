﻿namespace ByteStorm.PassthroughDrive.Console
{
    using System.Diagnostics;
    using Dokan;
    using System;
    using System.IO;

    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return mount();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Terminating due to above error.");
                //Console.ReadKey();
                return 1;
            }
        }

        public static int mount() {
            System.Console.BackgroundColor = ConsoleColor.White;
            System.Console.ForegroundColor = ConsoleColor.DarkBlue;
            System.Console.WriteLine("Welcome to ByteStorm Crypto Drive");
            System.Console.ResetColor();
            DokanOptions opt = new DokanOptions();
            opt.MountPoint = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_DRIVELETTER, CryptoConfiguration.DEFAULT_DRIVELETTER);//Properties.Settings.Default.DriveLetter;
            opt.DebugMode = false;
            opt.UseStdErr = false;
            opt.VolumeLabel = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_VOLUMELABEL, "ByteStormDrive");//Properties.Settings.Default.VolumeLabel;
            string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ByteStorm", "CryptoDrive", "mappings.db");

            byte[] key, iv;

            if (loadKeyAndIV(out key, out iv))
            {
                string mountPath = loadMountPath();
                System.Console.WriteLine("Mounting " + mountPath + " as drive " + opt.MountPoint + ":");
                CryptViewOperations cwo = new CryptViewOperations(mountPath, dbpath, key, iv);

                Console.CancelKeyPress += delegate
                {
                    cwo.shutdown();
                };

                int status = DokanNet.DokanMain(opt, cwo);
                switch (status)
                {
                    case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                        Trace.WriteLine("Drive letter error");
                        break;
                    case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                        Trace.WriteLine("Driver install error");
                        break;
                    case DokanNet.DOKAN_MOUNT_ERROR:
                        Trace.WriteLine("Mount error");
                        break;
                    case DokanNet.DOKAN_START_ERROR:
                        Trace.WriteLine("Start error");
                        break;
                    case DokanNet.DOKAN_ERROR:
                        Trace.WriteLine("Unknown error");
                        break;
                    case DokanNet.DOKAN_SUCCESS:
                        Trace.WriteLine("Success");
                        break;
                    default:
                        Trace.WriteLine(string.Format("Unknown status: %d", status));
                        break;

                }
                System.Console.ReadLine();
            }
            else
            {
                System.Console.WriteLine("Could not load settings file. Terminating.");
                return 1;
            }
            return 0;
        }

        private static string loadMountPath()
        {
            bool newPath = false;
            string path = CryptoConfiguration.Instance.getSetting(CryptoConfiguration.KEY_MOUNTPATH, null);
            while (path == null || !(new DirectoryInfo(path).Exists))
            {
                System.Console.Write("Enter path to mount: ");
                path = System.Console.ReadLine();
                newPath = true;
            }
            if (newPath)
            {
                CryptoConfiguration.Instance.setSetting(CryptoConfiguration.KEY_MOUNTPATH, path);
                CryptoConfiguration.Instance.save();
            }
            return path;
        }

        static bool loadKeyAndIV(out byte[] key, out byte[] iv) {
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
            /*try
            {
                var UserConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                return UserConfig.FilePath;
            }
            catch (ConfigurationException e)
            {
                return e.Filename;
            }*/
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
                /*Properties.Settings.Default.key = keyString;
                Properties.Settings.Default.iv = ivString;
                Properties.Settings.Default.Save();*/
                return true;
            }
            catch
            {
                Trace.WriteLine("Conversion error");
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
    }
}
