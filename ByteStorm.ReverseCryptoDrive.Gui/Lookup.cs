using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ByteStorm.ReverseCryptoDrive.Gui
{
    public partial class Lookup : Form
    {
        private PassthroughDrive.CryptViewOperations cwo;

        public Lookup(PassthroughDrive.CryptViewOperations cwo)
        {
            this.cwo = cwo;
            InitializeComponent();
        }

        private void btnToEncrypted_Click(object sender, EventArgs e)
        {
            string[] lines = txtPlain.Lines;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = cwo.translateToPathOfIds(lines[i], null);
            }
            txtEncrypted.Lines = lines;
        }

        private void btnToPlain_Click(object sender, EventArgs e)
        {
            string[] lines = txtEncrypted.Lines;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = cwo.translateToPathOfNames(lines[i]);
            }
            txtPlain.Lines = lines;
        }
    }
}
